using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;

namespace EXE201_Backend.Repositories
{
    public interface IWorkshopRepository
    {
        Task AddAsync(Workshop workshop, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedResultDto<Workshop>> GetAllPagedAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<PagedResultDto<Workshop>> GetByUserIdPagedAsync(int userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<Workshop?> GetByIdAsync(int id, int? userId = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Workshop>> GetRecommendationsAsync(int? userId, CancellationToken cancellationToken = default);
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
        Task<PagedResultDto<Workshop>> SearchAsync(string? query = null, IEnumerable<string>? locations = null, IEnumerable<string>? categories = null, IEnumerable<string>? levels = null, decimal? priceMin = null, decimal? priceMax = null, int? durationMin = null, int? durationMax = null, int? scheduleWithinDays = null, WorkshopSort? sortBy = null, bool sortDesc = false, int userId = 0, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(Workshop workshop, CancellationToken cancellationToken = default);
    }
}