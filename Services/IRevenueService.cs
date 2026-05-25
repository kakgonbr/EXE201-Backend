using EXE201_Backend.Models.Dto;
using EXE201_Backend.Repositories;

namespace EXE201_Backend.Services
{
    public interface IRevenueService
    {
        Task<bool> CreateWithdrawRequestAsync(int hostId, decimal amount, string bankName, string bankAccount, CancellationToken cancellationToken = default);
        Task<RevenueStatisticsDto> GetRevenueStatisticsAsync(int hostId, CancellationToken cancellationToken = default);
        Task<PagedResultDto<HostWithdrawRequestDto>> GetWithdrawRequestsAsync(WithdrawStatusFilter? statusFilter = null, HostWithdrawSort? sortBy = null, bool sortDesc = false, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<bool> UpdateWithdrawRequestAsync(int requestId, string newStatus, string? note, CancellationToken cancellationToken = default);
    }
}