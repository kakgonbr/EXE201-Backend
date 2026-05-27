using AutoMapper;
using EXE201_Backend.Models.Dto;
using EXE201_Backend.Repositories;
using System.Collections.Concurrent;

namespace EXE201_Backend.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IWorkshopParticipantRepository _workshopParticipantRepository;
        private readonly IWorkshopTicketRepository _workshopTicketRepository;
        private readonly IConfigurationService _configurationService;
        private readonly ITimeProvider _time_provider;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PaymentService> _logger;

        private static ILogger? _staticLogger;

        private static readonly ConcurrentDictionary<(int userId, int ticketId), PaymentState> _paymentStates = new();
        private static readonly object _cleanupLock = new();
        private static bool _cleanupStarted = false;
        private static CancellationTokenSource? _cleanupCts;
        private static IServiceScopeFactory? _staticScopeFactory;
        private static ITimeProvider? _staticTimeProvider;

        private record PaymentState(DateTime CreatedAt, DateTime ExpiresAt);

        public PaymentService(
            IWorkshopParticipantRepository workshopParticipantRepository,
            IWorkshopTicketRepository workshopTicketRepository,
            IConfigurationService configurationService,
            ITimeProvider timeProvider,
            IPaymentRepository paymentRepository,
            IMapper mapper,
            IServiceScopeFactory scopeFactory,
            ILogger<PaymentService> logger)
        {
            _workshopParticipantRepository = workshopParticipantRepository;
            _workshopTicketRepository = workshopTicketRepository;
            _configurationService = configurationService;
            _time_provider = timeProvider;
            _paymentRepository = paymentRepository;
            _mapper = mapper;
            _scopeFactory = scopeFactory;
            _logger = logger;

            _staticLogger ??= logger;

            lock (_cleanupLock)
            {
                if (!_cleanupStarted)
                {
                    _staticTimeProvider = timeProvider;
                    _staticScopeFactory = scopeFactory;
                    _cleanupCts = new CancellationTokenSource();
                    _ = Task.Run(() => CleanupLoopAsync(_cleanupCts.Token));
                    _cleanupStarted = true;
                    _staticLogger?.LogInformation("PaymentService cleanup loop started.");
                }
            }
        }

        public async Task<PaymentInfoDto?> StartCheckout(int userId, int ticketId, CancellationToken cancellationToken = default)
        {
            try
            {
                var ticket = await _workshopTicketRepository.GetByIdAsync(ticketId, cancellationToken);
                if (ticket == null)
                {
                    _logger.LogWarning("ticket {TicketId} not found for user {UserId}.", ticketId, userId);
                    return null;
                }

                var expectedAmount = Math.Round(CalculateServiceCost(ticket.Price));

                try
                {
                    var existingPayment = await _paymentRepository.GetByIdAsync(userId, ticketId, cancellationToken);
                    if (existingPayment == null)
                    {
                        var payment = new Models.Payment
                        {
                            ParticipantId = userId,
                            TicketId = ticketId,
                            Amount = expectedAmount,
                            Status = "pending",
                            CreatedOn = _time_provider.Now
                        };

                        await _paymentRepository.AddAsync(payment, cancellationToken);
                        _logger.LogInformation("created Payment (UserId={UserId}, TicketId={TicketId}, Amount={Amount}, Status=pending).", userId, ticketId, expectedAmount);
                    }
                    else
                    {
                        existingPayment.Amount = expectedAmount;
                        existingPayment.Status = "pending";
                        existingPayment.CreatedOn = _time_provider.Now;
                        await _paymentRepository.UpdateAsync(existingPayment, cancellationToken);
                        _logger.LogInformation("updated existing Payment to pending (UserId={UserId}, TicketId={TicketId}).", userId, ticketId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "failed to create/update Payment record for (UserId={UserId}, TicketId={TicketId}).", userId, ticketId);
                }

                var expireSeconds = Math.Max(1, _configurationService.PAYMENT_EXPIRE_SEC);
                var created = _time_provider.Now;
                var expiresAt = created.AddSeconds(expireSeconds);

                var key = (userId, ticketId);
                var state = new PaymentState(created, expiresAt);
                _paymentStates.AddOrUpdate(key, state, (_, __) => state);

                _logger.LogInformation("persisted transient payment state for (UserId={UserId}, TicketId={TicketId}) expires at {ExpiresAt}.", userId, ticketId, expiresAt);

                PaymentInfoDto paymentInfo = new()
                {
                    OrderAmount = expectedAmount.ToString("F0"),
                    Merchant = _configurationService.SE_MERCHANT,
                    Currency = "VND",
                    Operation = "PURCHASE",
                    OrderDescription = $"Payment for Workshop Ticket #{ticketId}",
                    OrderInvoiceNumber = $"INV_{userId}_{ticketId}_{created.Ticks}",
                    SuccessUrl = $"{_configurationService.SE_RETURN}?invoice_num=INV_{userId}_{ticketId}_{created.Ticks}",
                    ErrorUrl = _configurationService.SE_ERROR,
                    CancelUrl = _configurationService.SE_CANCEL
                };

                paymentInfo.Signature = GetSignature(paymentInfo, _configurationService.SE_SECRET);

                _logger.LogInformation("StartCheckout signature verification info - RawData: order_amount={OrderAmount},merchant={Merchant},currency={Currency},operation={Operation},order_description={OrderDescription},order_invoice_number={OrderInvoiceNumber},success_url={SuccessUrl},error_url={ErrorUrl},cancel_url={CancelUrl}. Secret Length: {SecretLen}. Signature: {Signature}", 
                    paymentInfo.OrderAmount, paymentInfo.Merchant, paymentInfo.Currency, paymentInfo.Operation, paymentInfo.OrderDescription, paymentInfo.OrderInvoiceNumber, paymentInfo.SuccessUrl, paymentInfo.ErrorUrl, paymentInfo.CancelUrl, _configurationService.SE_SECRET?.Length ?? 0, paymentInfo.Signature);

                return paymentInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unexpected error for (UserId={UserId}, TicketId={TicketId}).", userId, ticketId);
                return null;
            }
        }

        private decimal CalculateServiceCost(decimal basePrice) =>
            basePrice;

        public async Task<bool> InformPaymentStatus(int userId, int ticketId, decimal amountPaid, CancellationToken cancellationToken = default)
        {
            try
            {
                var ticket = await _workshopTicketRepository.GetByIdAsync(ticketId, cancellationToken);

                if (ticket == null)
                {
                    _logger.LogWarning("ticket {TicketId} not found for user {UserId}.", ticketId, userId);
                    return false;
                }

                var expectedAmount = Math.Round(CalculateServiceCost(ticket.Price));

                if (amountPaid != expectedAmount)
                {
                    _logger.LogInformation("amount mismatch (expected {Expected}, got {Paid}) for (UserId={UserId}, TicketId={TicketId}).", expectedAmount, amountPaid, userId, ticketId);
                    return await HandlePaymentFailure(userId, ticketId, amountPaid, cancellationToken);
                }

                var success = await HandlePaymentSuccess(userId, ticketId, amountPaid, cancellationToken);
                _logger.LogInformation("payment handling result for (UserId={UserId}, TicketId={TicketId}) => {Result}.", userId, ticketId, success ? "success" : "failure");
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unexpected error for (UserId={UserId}, TicketId={TicketId}).", userId, ticketId);
                return false;
            }
        }

        private async Task<bool> HandlePaymentFailure(int userId, int ticketId, decimal? amountPaid = null, CancellationToken cancellationToken = default)
        {
            _paymentStates.TryRemove((userId, ticketId), out _);
            _logger.LogDebug("removed transient state for (UserId={UserId}, TicketId={TicketId}).", userId, ticketId);

            try
            {
                var payment = await _paymentRepository.GetByIdAsync(userId, ticketId, cancellationToken);
                if (payment == null)
                {
                    var amount = amountPaid ?? 0m;
                    var newPayment = new Models.Payment
                    {
                        ParticipantId = userId,
                        TicketId = ticketId,
                        Amount = amount,
                        Status = "failed",
                        CreatedOn = _time_provider.Now
                    };

                    await _paymentRepository.AddAsync(newPayment, cancellationToken);
                    _logger.LogInformation("created failed Payment record (UserId={UserId}, TicketId={TicketId}, Amount={Amount}).", userId, ticketId, amount);
                }
                else
                {
                    payment.Status = "failed";
                    if (amountPaid.HasValue) payment.Amount = amountPaid.Value;
                    payment.CreatedOn = _time_provider.Now;
                    await _paymentRepository.UpdateAsync(payment, cancellationToken);
                    _logger.LogInformation("updated Payment to failed (UserId={UserId}, TicketId={TicketId}).", userId, ticketId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "failed to persist failed Payment for (UserId={UserId}, TicketId={TicketId}).", userId, ticketId);
            }

            try
            {
                await _workshopParticipantRepository.DeleteAsync(userId, ticketId, cancellationToken);
                _logger.LogInformation("deleted WorkshopParticipant (UserId={UserId}, TicketId={TicketId}) if existed.", userId, ticketId);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "deleting WorkshopParticipant via injected repo failed; attempting scoped repo.");

                if (_staticScopeFactory != null)
                {
                    try
                    {
                        using var scope = _staticScopeFactory.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<IWorkshopParticipantRepository>();
                        await repo.DeleteAsync(userId, ticketId, cancellationToken);
                        _staticLogger?.LogInformation("deleted WorkshopParticipant via scoped repo (UserId={UserId}, TicketId={TicketId}).", userId, ticketId);
                    }
                    catch (Exception ex2)
                    {
                        _staticLogger?.LogWarning(ex2, "scoped repo delete failed for (UserId={UserId}, TicketId={TicketId}).", userId, ticketId);
                    }
                }
            }

            return true;
        }

        private async Task<bool> HandlePaymentSuccess(int userId, int ticketId, decimal amountPaid, CancellationToken cancellationToken = default)
        {
            var key = (userId, ticketId);

            if (!_paymentStates.TryRemove(key, out var state))
            {
                _logger.LogWarning("no transient state found for (UserId={UserId}, TicketId={TicketId}). Treating as failure.", userId, ticketId);
                return await HandlePaymentFailure(userId, ticketId, amountPaid, cancellationToken);
            }

            if (state.ExpiresAt <= _time_provider.Now)
            {
                _logger.LogInformation("transient state expired for (UserId={UserId}, TicketId={TicketId}).", userId, ticketId);
                return await HandlePaymentFailure(userId, ticketId, amountPaid, cancellationToken);
            }

            try
            {
                var payment = await _paymentRepository.GetByIdAsync(userId, ticketId, cancellationToken);
                if (payment == null)
                {
                    payment = new Models.Payment
                    {
                        ParticipantId = userId,
                        TicketId = ticketId,
                        Amount = amountPaid,
                        Status = "success",
                        CreatedOn = _time_provider.Now
                    };
                    await _paymentRepository.AddAsync(payment, cancellationToken);
                    _logger.LogInformation("created Payment record success (UserId={UserId}, TicketId={TicketId}, Amount={Amount}).", userId, ticketId, amountPaid);
                }
                else
                {
                    payment.Amount = amountPaid;
                    payment.Status = "success";
                    payment.CreatedOn = _time_provider.Now;
                    await _paymentRepository.UpdateAsync(payment, cancellationToken);
                    _logger.LogInformation("updated Payment to success (UserId={UserId}, TicketId={TicketId}).", userId, ticketId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "failed to persist Payment record for (UserId={UserId}, TicketId={TicketId}).", userId, ticketId);
            }

            try
            {
                var participant = new Models.WorkshopParticipant
                {
                    ParticipantId = userId,
                    TicketId = ticketId,
                    Status = "paid",
                    BookedOn = _time_provider.Now
                };

                await _workshopParticipantRepository.AddAsync(participant, cancellationToken);
                _logger.LogInformation("created WorkshopParticipant (UserId={UserId}, TicketId={TicketId}) with status 'paid'.", userId, ticketId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to create WorkshopParticipant for (UserId={UserId}, TicketId={TicketId}).", userId, ticketId);
                return false;
            }
        }

        public async Task<string?> GetPaymentStatusAsync(int userId, int ticketId, CancellationToken cancellationToken = default)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(userId, ticketId, cancellationToken);
                if (payment == null)
                {
                    _logger.LogInformation("no Payment record found for (UserId={UserId}, TicketId={TicketId}).", userId, ticketId);
                    return null;
                }

                _logger.LogInformation("found Payment for (UserId={UserId}, TicketId={TicketId}) with Status={Status}.", userId, ticketId, payment.Status);
                return payment.Status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unexpected error while checking payment for (UserId={UserId}, TicketId={TicketId}).", userId, ticketId);
                return null;
            }
        }
        
        private static async Task CleanupLoopAsync(CancellationToken cancellationToken)
        {
            var scanIntervalMs = 5_000;
            var log = _staticLogger;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var expiredKeys = new List<(int userId, int ticketId)>();

                    foreach (var kvp in _paymentStates)
                    {
                        if (kvp.Value.ExpiresAt <= _staticTimeProvider!.Now)
                        {
                            expiredKeys.Add(kvp.Key);
                        }
                    }

                    if (expiredKeys.Count > 0)
                    {
                        log?.LogInformation("CleanupLoop: found {Count} expired payment states.", expiredKeys.Count);

                        foreach (var key in expiredKeys)
                        {
                            if (_paymentStates.TryRemove(key, out _))
                            {
                                log?.LogDebug("CleanupLoop: removed in-memory state for (UserId={UserId}, TicketId={TicketId}).", key.userId, key.ticketId);

                                if (_staticScopeFactory != null)
                                {
                                    try
                                    {
                                        using var scope = _staticScopeFactory.CreateScope();
                                        var paymentRepo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
                                        var participantRepo = scope.ServiceProvider.GetRequiredService<IWorkshopParticipantRepository>();

                                        var payment = await paymentRepo.GetByIdAsync(key.userId, key.ticketId, cancellationToken);
                                        if (payment == null)
                                        {
                                            var failed = new Models.Payment
                                            {
                                                ParticipantId = key.userId,
                                                TicketId = key.ticketId,
                                                Amount = 0m,
                                                Status = "failed",
                                                CreatedOn = _staticTimeProvider!.Now
                                            };
                                            await paymentRepo.AddAsync(failed, cancellationToken);
                                            log?.LogInformation("CleanupLoop: created failed Payment record for expired state (UserId={UserId}, TicketId={TicketId}).", key.userId, key.ticketId);
                                        }
                                        else
                                        {
                                            payment.Status = "failed";
                                            payment.CreatedOn = _staticTimeProvider!.Now;
                                            await paymentRepo.UpdateAsync(payment, cancellationToken);
                                            log?.LogInformation("CleanupLoop: updated Payment to failed for expired state (UserId={UserId}, TicketId={TicketId}).", key.userId, key.ticketId);
                                        }

                                        try
                                        {
                                            await participantRepo.DeleteAsync(key.userId, key.ticketId, cancellationToken);
                                            log?.LogInformation("CleanupLoop: deleted WorkshopParticipant for expired state (UserId={UserId}, TicketId={TicketId}).", key.userId, key.ticketId);
                                        }
                                        catch (Exception exDel)
                                        {
                                            log?.LogDebug(exDel, "CleanupLoop: deleting WorkshopParticipant for expired state failed (UserId={UserId}, TicketId={TicketId}).", key.userId, key.ticketId);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        log?.LogWarning(ex, "CleanupLoop: failed to persist failure for expired payment state (UserId={UserId}, TicketId={TicketId}).", key.userId, key.ticketId);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log?.LogError(ex, "CleanupLoop: unexpected error during cleanup iteration.");
                }

                try
                {
                    await Task.Delay(scanIntervalMs, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    log?.LogInformation("CleanupLoop: cancellation requested, exiting cleanup loop.");
                    break;
                }
            }
        }

        private static string GetSignature(PaymentInfoDto paymentInfo, string secret)
        {
            var rawData = $"order_amount={paymentInfo.OrderAmount},merchant={paymentInfo.Merchant},currency={paymentInfo.Currency},operation={paymentInfo.Operation},order_description={paymentInfo.OrderDescription},order_invoice_number={paymentInfo.OrderInvoiceNumber},success_url={paymentInfo.SuccessUrl},error_url={paymentInfo.ErrorUrl},cancel_url={paymentInfo.CancelUrl}";
            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
            var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
