
using EXE201_Backend.Models.Dto;
using EXE201_Backend.Repositories;

namespace EXE201_Backend.Services
{
    public interface IHostService
    {
        Task<PagedResultDto<HostRegistrationDto>> GetHostRegistrations(ApproveStatusFilter? approveFilter = null, HostRegistrationSort? sortBy = null, bool sortDesc = false, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<bool> RegisterHostAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> UpdateHostRegistrationAsync(int staffId, int userId, bool approved, string? note, CancellationToken cancellationToken = default);
    }
}