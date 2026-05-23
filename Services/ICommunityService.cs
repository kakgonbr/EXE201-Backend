
using EXE201_Backend.Models.Dto;

namespace EXE201_Backend.Services
{
    public interface ICommunityService
    {
        Task<PagedResultDto<WorkshopReviewDto>> GetWorkshopReviewsAsync(int workshopId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<bool> PostWorkshopReviewAsync(int userId, int workshopId, string title, string desc, int rating, CancellationToken cancellationToken = default);
        Task<bool> PostWorkshopReviewResponseAsync(int hostId, int workshopReviewId, string response, CancellationToken cancellationToken = default);
        Task<bool> ToggleLikeWorkshopAsync(int userId, int workshopId, CancellationToken cancellationToken = default);
    }
}