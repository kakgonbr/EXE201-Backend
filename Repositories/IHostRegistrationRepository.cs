using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;

namespace EXE201_Backend.Repositories
{
    public interface IHostRegistrationRepository
    {
        Task AddAsync(HostRegistration hostRegistration, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedResultDto<HostRegistration>> GetAllAsync(ApproveStatusFilter? approveFilter = null, HostRegistrationSort? sortBy = null, bool sortDesc = false, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<List<HostRegistration>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<HostRegistration?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<HostRegistration?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(HostRegistration hostRegistration, CancellationToken cancellationToken = default);
    }
}