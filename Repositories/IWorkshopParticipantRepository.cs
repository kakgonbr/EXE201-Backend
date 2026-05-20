using EXE201_Backend.Models;
using EXE201_Backend.Models.Responses;

namespace EXE201_Backend.Repositories
{
    public interface IWorkshopParticipantRepository
    {
        Task AddAsync(WorkshopParticipant participant, CancellationToken cancellationToken = default);
        Task DeleteAsync(int participantId, int ticketId, CancellationToken cancellationToken = default);
        Task<List<WorkshopParticipant>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<WorkshopParticipant>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<WorkshopParticipant?> GetByIdAsync(int participantId, int ticketId, CancellationToken cancellationToken = default);
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(WorkshopParticipant participant, CancellationToken cancellationToken = default);
    }
}