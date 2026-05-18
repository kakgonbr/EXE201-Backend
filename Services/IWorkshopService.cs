using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using EXE201_Backend.Repositories;

namespace EXE201_Backend.Services
{
    public interface IWorkshopService
    {
        Task<PagedResult<WorkshopDisplayDto>> GetWorkshopAsync(string? query = null, IEnumerable<string>? locations = null, IEnumerable<int>? categoryIds = null, IEnumerable<string>? levels = null, decimal? priceMin = null, decimal? priceMax = null, int? durationMin = null, int? durationMax = null, int? scheduleWithinDays = null, WorkshopSort? sortBy = null, bool sortDesc = false, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    }
}