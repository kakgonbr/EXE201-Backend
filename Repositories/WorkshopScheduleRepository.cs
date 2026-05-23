using EXE201_Backend.Data;
using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using EXE201_Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace EXE201_Backend.Repositories
{
    public class WorkshopScheduleRepository : IWorkshopScheduleRepository
    {
        private readonly ExeContext _db;
        private readonly ITimeProvider _timeProvider;

        public WorkshopScheduleRepository(ExeContext db, ITimeProvider timeProvider)
        {
            _db = db;
            _timeProvider = timeProvider;
        }

        public async Task<WorkshopSchedule?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.WorkshopSchedules
                .Include(ws => ws.Workshop)
                    .ThenInclude(w => w.CreatedByNavigation)
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

        public async Task<PagedResultDto<WorkshopSchedule>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _db.WorkshopSchedules.ToPagedResultAsync(page, pageSize, cancellationToken);
        }

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<WorkshopSchedule>> GetSchedulesInMonthAsync(int userId, int month, CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(_timeProvider.Now);
            var firstDayOfMonth = new DateOnly(today.Year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            return await _db.WorkshopSchedules
                .Include(ws => ws.Workshop)
                    .ThenInclude(w => w.CreatedByNavigation)
                .Include(ws => ws.WorkshopTickets)
                    .ThenInclude(wt => wt.WorkshopParticipants)
                .Where(ws =>
                    ws.StartOn >= today && ws.StartOn >= firstDayOfMonth && ws.StartOn <= lastDayOfMonth
                    && ws.WorkshopTickets.Any(wt => wt.WorkshopParticipants.Any(wp => wp.ParticipantId == userId && wp.Status == "paid")))
                .OrderBy(ws => ws.StartOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsUserOccupiedAsync(int userId, int ticketId, CancellationToken cancellationToken = default)
        {
            var ticket = await _db.WorkshopTickets
                .Include(wt => wt.WorkshopSchedule)
                .SingleOrDefaultAsync(wt => wt.Id == ticketId, cancellationToken);

            if (ticket == null)
            {
                return false;
            }

            return await _db.WorkshopSchedules
                .Include(ws => ws.WorkshopTickets)
                    .ThenInclude(wt => wt.WorkshopParticipants)
                .AnyAsync(ws =>
                ws.StartOn == ticket.WorkshopSchedule.StartOn
                && ws.WorkshopTickets.Any(wt => wt.WorkshopParticipants.Any(wp => wp.ParticipantId == userId && wp.Status == "paid")),
                cancellationToken);
        }

        public async Task<PagedResultDto<WorkshopSchedule>> GetUpcoming(int userId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(_timeProvider.Now);
            return await _db.WorkshopSchedules
                .Include(ws => ws.Workshop)
                    .ThenInclude(w => w.Category)
                .Include(ws => ws.Workshop)
                    .ThenInclude(w => w.Level)
                .Include(ws => ws.Workshop)
                    .ThenInclude(w => w.CreatedByNavigation)
                .Include(ws => ws.WorkshopTickets)
                    .ThenInclude(wt => wt.WorkshopParticipants)
                .Where(ws =>
                    ws.StartOn >= today
                    && ws.WorkshopTickets.Any(wt => wt.WorkshopParticipants.Any(wp => wp.ParticipantId == userId && wp.Status == "paid")))
                .OrderBy(ws => ws.StartOn)
                .ToPagedResultAsync(page, pageSize, cancellationToken);
        }
    }
}