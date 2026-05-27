using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;

namespace EXE201_Backend.Repositories
{
    public interface IWorkshopTicketRepository
    {
        Task AddAsync(WorkshopTicket ticket, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<List<WorkshopTicket>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResultDto<WorkshopTicket>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<WorkshopTicket?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(WorkshopTicket ticket, CancellationToken cancellationToken = default);
        Task<PagedResultDto<WorkshopTicket>> GetUpcomingTicketsAsync(int hostId, DateTime time, int page, int pageSize, CancellationToken cancellationToken = default);
    }
}