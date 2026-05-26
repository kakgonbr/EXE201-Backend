using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;

namespace EXE201_Backend.Repositories
{
    public interface IHostWithdrawRepository
    {
        Task AddAsync(HostWithdraw hostWithdraw, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<List<HostWithdraw>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResultDto<HostWithdraw>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<PagedResultDto<HostWithdraw>> GetAllAsync(WithdrawStatusFilter? statusFilter = null, HostWithdrawSort? sortBy = null, bool sortDesc = false, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<HostWithdraw?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(HostWithdraw hostWithdraw, CancellationToken cancellationToken = default);
    }
}