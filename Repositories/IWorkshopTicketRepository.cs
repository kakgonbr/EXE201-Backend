using EXE201_Backend.Models;
using EXE201_Backend.Models.Responses;

namespace EXE201_Backend.Repositories
{
    public interface IWorkshopTicketRepository
    {
        Task AddAsync(WorkshopTicket ticket, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<List<WorkshopTicket>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<WorkshopTicket>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<WorkshopTicket?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(WorkshopTicket ticket, CancellationToken cancellationToken = default);
    }
}