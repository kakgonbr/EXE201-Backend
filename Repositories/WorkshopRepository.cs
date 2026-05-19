using EXE201_Backend.Data;
using EXE201_Backend.Models;
using EXE201_Backend.Extensions;
using Microsoft.EntityFrameworkCore;
using EXE201_Backend.Services;
using EXE201_Backend.Models.Responses;

namespace EXE201_Backend.Repositories
{
    public enum WorkshopSort
    {
        Relevance = 0,
        Name,
        Price,
        Rating,
        Duration,
        Level
    }

    public class WorkshopRepository : IWorkshopRepository
    {
        private readonly ExeContext _db;
        private readonly ITimeProvider _timeProvider;

        public WorkshopRepository(ExeContext db, ITimeProvider timeProvider)
        {
            _db = db;
            _timeProvider = timeProvider;
        }

        public async Task<Workshop?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.Workshops
                .SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task AddAsync(Workshop workshop, CancellationToken cancellationToken = default)
        {
            await _db.Workshops.AddAsync(workshop, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> UpdateAsync(Workshop workshop, CancellationToken cancellationToken = default)
        {
            _db.Workshops.Update(workshop);
            return await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var workshop = await GetByIdAsync(id, cancellationToken);
            if (workshop != null)
            {
                _db.Workshops.Remove(workshop);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<Workshop>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Workshops.ToListAsync(cancellationToken);
        }

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<PagedResult<Workshop>> SearchAsync(
            string? query = null,
            IEnumerable<string>? locations = null,
            IEnumerable<string>? categories = null,
            IEnumerable<string>? levels = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int? durationMin = null,
            int? durationMax = null,
            int? scheduleWithinDays = null, // find workshops with schedules within next N days
            WorkshopSort? sortBy = null,
            bool sortDesc = false,
            int userId = 0,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var currentDate = DateOnly.FromDateTime(_timeProvider.Now);
            IQueryable<Workshop> q = _db.Workshops
                .AsQueryable()
                .Where(w => w.Status == "verified"
                && w.WorkshopSchedules.Count != 0
                && w.WorkshopSchedules.Any(ws => ws.StartOn > currentDate && ws.WorkshopTickets.Count != 0)
                )
                .Include(w => w.Category)
                .Include(w => w.Level)
                .Include(w => w.Users.Where(u => u.Id == userId))
                .Include(w => w.WorkshopReviews!)
                .Include(w => w.WorkshopImages!);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var trimmed = query.Trim();
                var like = $"%{trimmed}%";
                q = q.Where(w =>
                    EF.Functions.Like(w.Title, like) ||
                    (!string.IsNullOrEmpty(w.Description) && EF.Functions.Like(w.Description!, like)) ||
                    EF.Functions.Like(w.InstructorName, like) ||
                    (w.Category != null && EF.Functions.Like(w.Category.Name, like))
                );
            }

            if (locations != null && locations.Any())
            {
                var locList = locations.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
                if (locList.Count != 0)
                {
                    q = q.Where(w => locList.Contains(w.Location));
                }
            }

            if (categories != null && categories.Any())
            {
                var catList = categories.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
                if (catList.Count != 0)
                {
                    q = q.Where(w => catList.Contains(w.Category.Name));
                }
            }

            if (levels != null && levels.Any())
            {
                var levelList = levels.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
                if (levelList.Count != 0)
                {
                    q = q.Where(w => levelList.Contains(w.Level.Name));
                }
            }

            if (durationMin.HasValue)
            {
                q = q.Where(w => w.Duration >= durationMin.Value);
            }
            if (durationMax.HasValue)
            {
                q = q.Where(w => w.Duration <= durationMax.Value);
            }

            if (priceMin.HasValue)
            {
                q = q.Where(w =>
                    w.WorkshopSchedules!.Any(s =>
                        s.WorkshopTickets!.Any(t => t.Price >= priceMin.Value)
                    )
                );
            }
            if (priceMax.HasValue)
            {
                q = q.Where(w =>
                    w.WorkshopSchedules!.Any(s =>
                        s.WorkshopTickets!.Any(t => t.Price <= priceMax.Value)
                    )
                );
            }

            if (scheduleWithinDays.HasValue && scheduleWithinDays.Value >= 0)
            {
                var upper = currentDate.AddDays(scheduleWithinDays.Value);
                q = q.Where(w =>
                    w.WorkshopSchedules!.Any(ws => ws.StartOn <= upper))
                    .Include(w => w.WorkshopSchedules!.Where(ws => ws.StartOn > currentDate && ws.StartOn <= upper))
                    .ThenInclude(s => s.WorkshopTickets!);
            }
            else
            {
                q = q.Include(w => w.WorkshopSchedules!.Where(ws => ws.StartOn > currentDate))
                    .ThenInclude(s => s.WorkshopTickets!);
            }

                sortBy ??= WorkshopSort.Relevance;

            switch (sortBy.Value)
            {
                case WorkshopSort.Name:
                    q = sortDesc ? q.OrderByDescending(w => w.Title) : q.OrderBy(w => w.Title);
                    break;

                case WorkshopSort.Price:
                    q = sortDesc
                        ? q.OrderByDescending(w => w.WorkshopSchedules!
                                                    .SelectMany(s => s.WorkshopTickets!)
                                                    .Min(wt => wt.Price))
                        : q.OrderBy(w => w.WorkshopSchedules!
                                        .SelectMany(s => s.WorkshopTickets!)
                                        .Min(wt => wt.Price));
                    break;

                case WorkshopSort.Rating:
                    q = sortDesc
                        ? q.OrderByDescending(w => w.WorkshopReviews!.Any()
                                                    ? w.WorkshopReviews.Select(r => (double?)r.Rating).Average()
                                                    : 0)
                        : q.OrderBy(w => w.WorkshopReviews!.Any()
                                        ? w.WorkshopReviews.Select(r => (double?)r.Rating).Average()
                                        : 0);
                    break;

                case WorkshopSort.Duration:
                    q = sortDesc ? q.OrderByDescending(w => w.Duration) : q.OrderBy(w => w.Duration);
                    break;

                case WorkshopSort.Level:
                    q = sortDesc ? q.OrderByDescending(w => w.Level.Id) : q.OrderBy(w => w.Level.Id);
                    break;

                case WorkshopSort.Relevance:
                default:
                    q = q.OrderByDescending(w => w.CreatedOn);
                    break;
            }

            return await q.ToPagedResultAsync(page, pageSize, cancellationToken);
        }
    }
}
