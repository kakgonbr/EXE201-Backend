using EXE201_Backend.Data;
using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace EXE201_Backend.Repositories
{
    public class WorkshopScheduleRepository : IWorkshopScheduleRepository
    {
        private readonly ExeContext _db;

        public WorkshopScheduleRepository(ExeContext db)
        {
            _db = db;
        }

        public async Task<WorkshopSchedule?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.WorkshopSchedules
                .Include(ws => ws.Workshop)
                .Include(ws => ws.WorkshopTickets)
                    .ThenInclude(wt => wt.WorkshopParticipants)
                .SingleOrDefaultAsync(ws => ws.Id == id, cancellationToken);
        }

        public async Task AddAsync(WorkshopSchedule schedule, CancellationToken cancellationToken = default)
        {
            await _db.WorkshopSchedules.AddAsync(schedule, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> UpdateAsync(WorkshopSchedule schedule, CancellationToken cancellationToken = default)
        {
            _db.WorkshopSchedules.Update(schedule);
            return await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var schedule = await GetByIdAsync(id, cancellationToken);
            if (schedule != null)
            {
                _db.WorkshopSchedules.Remove(schedule);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<WorkshopSchedule>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.WorkshopSchedules.ToListAsync(cancellationToken);
        }

        public async Task<PagedResult<WorkshopSchedule>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _db.WorkshopSchedules.ToPagedResultAsync(page, pageSize, cancellationToken);
        }

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
