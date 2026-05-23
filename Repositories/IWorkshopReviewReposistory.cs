using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;

namespace EXE201_Backend.Repositories
{
    public interface IWorkshopReviewReposistory
    {
        Task AddAsync(WorkshopReview workshopReview, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<List<WorkshopReview>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResultDto<WorkshopReview>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<PagedResultDto<WorkshopReview>> GetByHostIdAsync(int hostId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<WorkshopReview?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedResultDto<WorkshopReview>> GetByWorkshopIdAsync(int workshopId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(WorkshopReview workshopReview, CancellationToken cancellationToken = default);
    }
}