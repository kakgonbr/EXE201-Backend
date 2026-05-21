using EXE201_Backend.Data;
using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace EXE201_Backend.Repositories
{
    public class WorkshopTicketRepository : IWorkshopTicketRepository
    {
        private readonly ExeContext _db;

        public WorkshopTicketRepository(ExeContext db)
        {
            _db = db;
        }

        public async Task<WorkshopTicket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.WorkshopTickets
                .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task AddAsync(WorkshopTicket ticket, CancellationToken cancellationToken = default)
        {
            await _db.WorkshopTickets.AddAsync(ticket, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> UpdateAsync(WorkshopTicket ticket, CancellationToken cancellationToken = default)
        {
            _db.WorkshopTickets.Update(ticket);
            return await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var ticket = await GetByIdAsync(id, cancellationToken);
            if (ticket != null)
            {
                _db.WorkshopTickets.Remove(ticket);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<WorkshopTicket>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.WorkshopTickets.ToListAsync(cancellationToken);
        }

        public async Task<PagedResultDto<WorkshopTicket>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _db.WorkshopTickets.ToPagedResultAsync(page, pageSize, cancellationToken);
        }

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
