using EXE201_Backend.Models;

namespace EXE201_Backend.Repositories
{
    public interface IWorkshopRepository
    {
        Task AddAsync(Workshop workshop, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Workshop>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Workshop?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<Workshop>> SearchAsync(string? query = null, IEnumerable<string>? locations = null, IEnumerable<int>? categoryIds = null, IEnumerable<string>? levels = null, decimal? priceMin = null, decimal? priceMax = null, int? durationMin = null, int? durationMax = null, int? scheduleWithinDays = null, WorkshopSort? sortBy = null, bool sortDesc = false, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(Workshop workshop, CancellationToken cancellationToken = default);
    }
}