using AutoMapper;
using EXE201_Backend.Models.Responses;
using EXE201_Backend.Repositories;
using System.Collections.Concurrent;

namespace EXE201_Backend.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IWorkshopParticipantRepository _workshopParticipantRepository;
        private readonly IWorkshopTicketRepository _workshopTicketRepository;
        private readonly IConfigurationService _configurationService;
        private readonly ITimeProvider _timeProvider;
        private readonly IMapper _mapper;
        private readonly IServiceScopeFactory _scopeFactory;

        private static readonly ConcurrentDictionary<(int userId, int ticketId), PaymentState> _paymentStates = new();
        private static readonly object _cleanupLock = new();
        private static bool _cleanupStarted = false;
        private static CancellationTokenSource? _cleanupCts;
        private static IServiceScopeFactory? _staticScopeFactory;
        private static ITimeProvider _staticTimeProvider;

        private record PaymentState(DateTime CreatedAt, DateTime ExpiresAt);

        public PaymentService(
            IWorkshopParticipantRepository workshopParticipantRepository,
            IWorkshopTicketRepository workshopTicketRepository,
            IConfigurationService configurationService,
            ITimeProvider timeProvider,
            IMapper mapper,
            IServiceScopeFactory scopeFactory)
        {
            _workshopParticipantRepository = workshopParticipantRepository;
            _workshopTicketRepository = workshopTicketRepository;
            _configurationService = configurationService;
            _timeProvider = timeProvider;
            _mapper = mapper;
            _scopeFactory = scopeFactory;

            lock (_cleanupLock)
            {
                if (!_cleanupStarted)
                {
                    _staticTimeProvider = timeProvider;
                    _staticScopeFactory = scopeFactory;
                    _cleanupCts = new CancellationTokenSource();
                    _ = Task.Run(() => CleanupLoopAsync(_cleanupCts.Token));
                    _cleanupStarted = true;
                }
            }
        }

        public async Task<PaymentInfo?> StartCheckout(int userId, int ticketId, CancellationToken cancellationToken = default)
        {
            var ticket = await _workshopTicketRepository.GetByIdAsync(ticketId, cancellationToken);
            if (ticket == null)
            {
                return null;
            }

            var participant = await _workshopParticipantRepository.GetByIdAsync(userId, ticketId, cancellationToken);

            if (participant == null)
            {
                participant = new Models.WorkshopParticipant
                {
                    ParticipantId = userId,
                    TicketId = ticketId,
                    Status = "unpaid",
                    BookedOn = _timeProvider.Now
                };

                await _workshopParticipantRepository.AddAsync(participant, cancellationToken);
                await _workshopParticipantRepository.SaveAsync(cancellationToken);
            }
            else
            {
                if (!string.Equals(participant.Status, "unpaid", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
            }

            var expireSeconds = Math.Max(1, _configurationService.PAYMENT_EXPIRE_SEC);
            var created = _timeProvider.Now;
            var expiresAt = created.AddSeconds(expireSeconds);

            var key = (userId, ticketId);
            var state = new PaymentState(created, expiresAt);
            _paymentStates.AddOrUpdate(key, state, (_, __) => state);

            PaymentInfo paymentInfo = new()
            {
                OrderAmount = (ticket.Price + _configurationService.SERVICE_COST_PERCENTAGE / 100m * ticket.Price).ToString("F2"),
                Merchant = _configurationService.SE_MERCHANT,
                Currency = "VND",
                Operation = "PURCHASE",
                OrderDescription = $"Payment for Workshop Ticket #{ticketId}",
                OrderInvoiceNumber = $"{userId}_{ticketId}_{created.Ticks}",
                SuccessUrl = _configurationService.SE_RETURN,
                ErrorUrl = _configurationService.SE_ERROR,
                CancelUrl = _configurationService.SE_CANCEL
            };

            paymentInfo.Signature = GetSignature(paymentInfo, _configurationService.SE_SECRET);

            return paymentInfo;
        }

        public async Task<bool> InformPaymentStatus(int userId, int ticketId, decimal amountPaid, CancellationToken cancellationToken = default)
        {
            var ticket = await _workshopTicketRepository.GetByIdAsync(ticketId, cancellationToken);

            if (ticket == null)
            {
                return false;
            }

            var expectedAmount = ticket.Price + _configurationService.SERVICE_COST_PERCENTAGE / 100m * ticket.Price;

            if (amountPaid != expectedAmount)
            {
                return await HandlePaymentFailure(userId, ticketId, cancellationToken);
            }

            return await HandlePaymentSuccess(userId, ticketId, amountPaid, cancellationToken);
        }

        private async Task<bool> HandlePaymentFailure(int userId, int ticketId, CancellationToken cancellationToken = default)
        {
            _paymentStates.TryRemove((userId, ticketId), out _);

            try
            {
                await _workshopParticipantRepository.DeleteAsync(userId, ticketId, cancellationToken);
                await _workshopParticipantRepository.SaveAsync(cancellationToken);
                return true;
            }
            catch
            {
                if (_staticScopeFactory != null)
                {
                    try
                    {
                        using var scope = _staticScopeFactory.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<IWorkshopParticipantRepository>();
                        await repo.DeleteAsync(userId, ticketId, cancellationToken);
                        await repo.SaveAsync(cancellationToken);
                        return true;
                    }
                    catch { }
                }
            }

            return false;
        }

        private async Task<bool> HandlePaymentSuccess(int userId, int ticketId, decimal amountPaid, CancellationToken cancellationToken = default)
        {
            var key = (userId, ticketId);

            if (!_paymentStates.TryRemove(key, out var state))
            {
                return await HandlePaymentFailure(userId, ticketId, cancellationToken);
            }

            if (state.ExpiresAt <= _timeProvider.Now)
            {
                return await HandlePaymentFailure(userId, ticketId, cancellationToken);
            }

            var participant = await _workshopParticipantRepository.GetByIdAsync(userId, ticketId, cancellationToken);
            if (participant == null || !string.Equals(participant.Status, "unpaid", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            participant.Status = "paid";

            participant.Payments ??= new List<Models.Payment>();
            participant.Payments.Add(new Models.Payment
            {
                ParticipantId = userId,
                TicketId = ticketId,
                Amount = amountPaid,
                Status = "success",
                CreatedOn = _timeProvider.Now
            });

            await _workshopParticipantRepository.UpdateAsync(participant, cancellationToken);
            await _workshopParticipantRepository.SaveAsync(cancellationToken);
            return true;
        }

        private static async Task CleanupLoopAsync(CancellationToken cancellationToken)
        {
            var scanIntervalMs = 5_000;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var expiredKeys = new List<(int userId, int ticketId)>();

                    foreach (var kvp in _paymentStates)
                    {
                        if (kvp.Value.ExpiresAt <= _staticTimeProvider.Now)
                        {
                            expiredKeys.Add(kvp.Key);
                        }
                    }

                    if (expiredKeys.Count > 0)
                    {
                        foreach (var key in expiredKeys)
                        {
                            if (_paymentStates.TryRemove(key, out _))
                            {
                                if (_staticScopeFactory != null)
                                {
                                    try
                                    {
                                        using var scope = _staticScopeFactory.CreateScope();
                                        var repo = scope.ServiceProvider.GetRequiredService<IWorkshopParticipantRepository>();
                                        await repo.DeleteAsync(key.userId, key.ticketId, cancellationToken);
                                        await repo.SaveAsync(cancellationToken);
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }
                catch
                {
                }

                try
                {
                    await Task.Delay(scanIntervalMs, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private string GetSignature(PaymentInfo paymentInfo, string secret)
        {
            var rawData = $"order_amount={paymentInfo.OrderAmount},merchant={paymentInfo.Merchant},currency={paymentInfo.Currency},operation={paymentInfo.Operation},order_description={paymentInfo.OrderDescription},order_invoice_number={paymentInfo.OrderInvoiceNumber},success_url={paymentInfo.SuccessUrl},error_url={paymentInfo.ErrorUrl},cancel_url={paymentInfo.CancelUrl}";
            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
            var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
