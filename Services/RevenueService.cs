using AutoMapper;
using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using EXE201_Backend.Repositories;

namespace EXE201_Backend.Services
{
    public class RevenueService : IRevenueService
    {
        private readonly IWorkshopRepository _workshopRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHostWithdrawRepository _hostWithdrawRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IConfigurationService _configurationService;
        private readonly IMailService _mailService;
        private readonly IMapper _mapper;

        public RevenueService(IWorkshopRepository workshopRepository, IHostWithdrawRepository hostWithdrawRepository, ITimeProvider timeProvider, IConfigurationService configurationService, IMapper mapper, IMailService mailService, IUserRepository userRepository)
        {
            _workshopRepository = workshopRepository;
            _configurationService = configurationService;
            _mapper = mapper;
            _hostWithdrawRepository = hostWithdrawRepository;
            _timeProvider = timeProvider;
            _mailService = mailService;
            _userRepository = userRepository;
        }

        public async Task<bool> CreateWithdrawRequestAsync(int hostId, decimal amount, string bankName, string bankAccount, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(hostId, cancellationToken);

            if (user == null)
            {
                return false;
            }

            var availableRevenue = await CalculateAvailableRevenueAsync(hostId, cancellationToken);

            if (amount <= 0 || amount > availableRevenue)
            {
                return false;
            }

            var withdrawRequest = new HostWithdraw
            {
                UserId = hostId,
                Amount = amount,
                CreatedOn = DateTime.UtcNow,
                BankAccount = bankAccount,
                BankName = bankName,
                Status = "pending"
            };

            await _hostWithdrawRepository.AddAsync(withdrawRequest, cancellationToken);

            await _mailService.SendWithdrawRequestReceived(user.Email, cancellationToken);

            return true;
        }

        public async Task<bool> UpdateWithdrawRequestAsync(int requestId, string newStatus, string? note, CancellationToken cancellationToken = default)
        {
            var request = await _hostWithdrawRepository.GetByIdAsync(requestId, cancellationToken);

            if (request == null)
            {
                return false;
            }

            if (newStatus == "approved")
            {
                var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
                if (user != null)
                {
                    await _mailService.SendWithdrawRequestApproved(user.Email, cancellationToken);
                }
            }
            else if (newStatus == "rejected")
            {
                var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
                if (user != null)
                {
                    await _mailService.SendWithdrawRequestRejected(user.Email, cancellationToken);
                }
            }

            request.Status = newStatus;
            request.Note = note;
            await _hostWithdrawRepository.UpdateAsync(request, cancellationToken);

            return true;
        }

        public async Task<PagedResultDto<HostWithdrawRequestDto>> GetWithdrawRequestsAsync(
            WithdrawStatusFilter? statusFilter = null,
            HostWithdrawSort? sortBy = null,
            bool sortDesc = false,
            int page = 1,
            int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var requests = await _hostWithdrawRepository.GetAllAsync(statusFilter, sortBy, sortDesc, page, pageSize, cancellationToken);

            return _mapper.MapPagedResult<HostWithdraw, HostWithdrawRequestDto>(requests);
        }

        public async Task<RevenueStatisticsDto> GetRevenueStatisticsAsync(int hostId, CancellationToken cancellationToken = default)
        {
            var totalRevenue = await CalculateTotalRevenueAsync(hostId, cancellationToken);
            var availableRevenue = await CalculateAvailableRevenueAsync(hostId, cancellationToken);
            var upcomingRevenue = await CalculateUpcomingRevenueAsync(hostId, cancellationToken);

            return new RevenueStatisticsDto
            {
                TotalRevenue = totalRevenue,
                AvailableRevenue = availableRevenue,
                UpcomingRevenue = upcomingRevenue
            };
        }

        private decimal FinalRevenue(decimal revenue) => ((100 - _configurationService.SERVICE_COST_PERCENTAGE) / 100.0m) * revenue;

        // TODO: CURERNTLY SLOW AND INEFFICIENT, SWAP TO BETTER QUERIES WITHIN REPOSITORIES

        private async Task<decimal> CalculateTotalRevenueAsync(int hostId, CancellationToken cancellationToken = default)
        {
            var currentDate = DateOnly.FromDateTime(_timeProvider.Now);
            var workshops = await _workshopRepository.GetByUserIdAsync(hostId, cancellationToken);

            decimal total = 0m;

            foreach (var w in workshops)
            {
                var schedules = w.WorkshopSchedules ?? Enumerable.Empty<WorkshopSchedule>();
                foreach (var sch in schedules)
                {
                    if (sch.StartOn < currentDate)
                    {
                        var tickets = sch.WorkshopTickets ?? Enumerable.Empty<WorkshopTicket>();
                        foreach (var t in tickets)
                        {
                            var participants = t.WorkshopParticipants ?? Enumerable.Empty<WorkshopParticipant>();
                            var checkedInCount = participants.Count(p =>
                                p.Status == "checked in" || p.Status == "paid");
                            total += t.Price * checkedInCount;
                        }
                    }
                }
            }

            return FinalRevenue(total);
        }

        private async Task<decimal> CalculateAvailableRevenueAsync(int hostId, CancellationToken cancellationToken = default)
        {
            var total = await CalculateTotalRevenueAsync(hostId, cancellationToken);

            var allWithdraws = await _hostWithdrawRepository.GetAllAsync(cancellationToken);
            var approvedForHost = allWithdraws
                .Where(h => h.UserId == hostId && string.Equals(h.Status, "approved", StringComparison.OrdinalIgnoreCase))
                .Sum(h => h.Amount);

            var available = total - approvedForHost;
            return available < 0 ? 0m : available;
        }

        private async Task<decimal> CalculateUpcomingRevenueAsync(int hostId, CancellationToken cancellationToken = default)
        {
            var currentDate = DateOnly.FromDateTime(_timeProvider.Now);
            var workshops = await _workshopRepository.GetByUserIdAsync(hostId, cancellationToken);

            decimal upcoming = 0m;

            foreach (var w in workshops)
            {
                var schedules = w.WorkshopSchedules ?? Enumerable.Empty<WorkshopSchedule>();
                foreach (var sch in schedules)
                {
                    if (sch.StartOn > currentDate) // future schedules
                    {
                        var tickets = sch.WorkshopTickets ?? Enumerable.Empty<WorkshopTicket>();
                        foreach (var t in tickets)
                        {
                            var participants = t.WorkshopParticipants ?? Enumerable.Empty<WorkshopParticipant>();
                            var paidCount = participants.Count(p => string.Equals(p.Status, "paid", StringComparison.OrdinalIgnoreCase));
                            upcoming += t.Price * paidCount;
                        }
                    }
                }
            }

            return FinalRevenue(upcoming);
        }
    }
}