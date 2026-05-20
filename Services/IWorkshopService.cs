using EXE201_Backend.Models.Dto;
using EXE201_Backend.Models.Responses;
using EXE201_Backend.Repositories;

namespace EXE201_Backend.Services
{
    public interface IWorkshopService
    {
        Task<IEnumerable<WorkshopDisplayDto>> GetRecommendedWorkshopsAsync(int? userId, CancellationToken cancellationToken = default);
        Task<WorkshopScheduleDetailsDto?> GetScheduleDetailsAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedResult<WorkshopDisplayDto>> GetWorkshopAsync(string? query = null, IEnumerable<string>? locations = null, IEnumerable<string>? categories = null, IEnumerable<string>? levels = null, decimal? priceMin = null, decimal? priceMax = null, int? durationMin = null, int? durationMax = null, int? scheduleWithinDays = null, WorkshopSort? sortBy = null, bool sortDesc = false, int userId = 0, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<WorkshopDetailsDto?> GetWorkshopByIdAsync(int id, int? userId = null, CancellationToken cancellationToken = default);
    }
}