using EXE201_Backend.Models.Dto;
using EXE201_Backend.Repositories;

namespace EXE201_Backend.Services
{
    public interface IWorkshopService
    {
        Task<IEnumerable<WorkshopDisplayDto>> GetRecommendedWorkshopsAsync(int? userId, CancellationToken cancellationToken = default);
        Task<PagedResultDto<WorkshopDisplayDto>> GetWorkshopAsync(string? query = null, IEnumerable<string>? locations = null, IEnumerable<string>? categories = null, IEnumerable<string>? levels = null, decimal? priceMin = null, decimal? priceMax = null, int? durationMin = null, int? durationMax = null, int? scheduleWithinDays = null, WorkshopSort? sortBy = null, bool sortDesc = false, int userId = 0, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<WorkshopDetailsDto?> GetWorkshopByIdAsync(int id, int? userId = null, CancellationToken cancellationToken = default);
        Task<int> CreateWorkshopAsync(Models.Requests.CreateWorkshopRequest request, int userId, CancellationToken cancellationToken = default);
        Task<PagedResultDto<WorkshopDetailsDto>> GetAllWorkshopsAsync(string? status = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<PagedResultDto<WorkshopDetailsDto>> GetWorkshopsByUserIdAsync(int userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<bool> UpdateWorkshopAsync(int id, Models.Requests.UpdateWorkshopRequest request, int userId, CancellationToken cancellationToken = default);
        Task<bool> DeleteWorkshopAsync(int id, int userId, CancellationToken cancellationToken = default);
        Task<bool> UpdateWorkshopApprovalAsync(int id, bool approved, CancellationToken cancellationToken = default);
        Task<PagedResultDto<WorkshopTicketDetailsDto>> GetTicketsAsync(int hostId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<bool> CheckInParticipantAsync(int ticketId, int participantId, int hostId, CancellationToken cancellationToken = default);
        Task<PagedResultDto<WorkshopParticipantDto>> GetParticipantsForTicketAsync(int ticketId, int hostId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    }
}