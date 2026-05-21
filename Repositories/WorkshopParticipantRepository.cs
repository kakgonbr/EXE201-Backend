using EXE201_Backend.Data;
using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace EXE201_Backend.Repositories
{
    public class WorkshopParticipantRepository : IWorkshopParticipantRepository
    {
        private readonly ExeContext _db;

        public WorkshopParticipantRepository(ExeContext db)
        {
            _db = db;
        }

        public async Task<WorkshopParticipant?> GetByIdAsync(int participantId, int ticketId, CancellationToken cancellationToken = default)
        {
            return await _db.WorkshopParticipants
                .Include(wp => wp.Participant)
                .Include(wp => wp.Ticket)
                    .ThenInclude(t => t.WorkshopSchedule)
                        .ThenInclude(ws => ws.Workshop)
                .SingleOrDefaultAsync(wp => wp.ParticipantId == participantId && wp.TicketId == ticketId, cancellationToken);
        }

        public async Task AddAsync(WorkshopParticipant participant, CancellationToken cancellationToken = default)
        {
            await _db.WorkshopParticipants.AddAsync(participant, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> UpdateAsync(WorkshopParticipant participant, CancellationToken cancellationToken = default)
        {
            _db.WorkshopParticipants.Update(participant);
            return await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int participantId, int ticketId, CancellationToken cancellationToken = default)
        {
            var participant = await GetByIdAsync(participantId, ticketId, cancellationToken);
            if (participant != null)
            {
                _db.WorkshopParticipants.Remove(participant);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<WorkshopParticipant>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.WorkshopParticipants.ToListAsync(cancellationToken);
        }

        public async Task<PagedResultDto<WorkshopParticipant>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _db.WorkshopParticipants.ToPagedResultAsync(page, pageSize, cancellationToken);
        }

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
