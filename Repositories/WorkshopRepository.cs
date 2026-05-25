using EXE201_Backend.Data;
using EXE201_Backend.Models;
using EXE201_Backend.Extensions;
using Microsoft.EntityFrameworkCore;
using EXE201_Backend.Services;
using EXE201_Backend.Models.Dto;

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

        public async Task<Workshop?> GetByIdAsync(int id, int? userId, CancellationToken cancellationToken = default)
        {
            IQueryable<Workshop> query = _db.Workshops
                .Include(w => w.Category)
                .Include(w => w.Level)
                .Include(w => w.WorkshopReviews!)
                .Include(w => w.WorkshopImages!)
                .Include(w => w.WorkshopSchedules!)
                    .ThenInclude(ws => ws.WorkshopTickets)
                        .ThenInclude(wt => wt.WorkshopParticipants);

            if (userId.HasValue && userId.Value > 0)
            {
                query = query.Include(w => w.Users.Where(u => u.Id == userId.Value));
            }

            return await query.SingleOrDefaultAsync(w => w.Id == id, cancellationToken);
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
            var workshop = await _db.Workshops
                .Include(w => w.WorkshopImages)
                .Include(w => w.WorkshopReviews)
                .Include(w => w.WorkshopScheduleConfig)
                .Include(w => w.WorkshopSchedules)
                    .ThenInclude(ws => ws.WorkshopTickets)
                        .ThenInclude(wt => wt.WorkshopParticipants)
                .Include(w => w.Users)
                .SingleOrDefaultAsync(w => w.Id == id, cancellationToken);

            if (workshop != null)
            {
                foreach (var schedule in workshop.WorkshopSchedules)
                {
                    if (schedule.WorkshopTickets != null && schedule.WorkshopTickets.Count > 0)
                    {
                        foreach (var ticket in schedule.WorkshopTickets)
                        {
                            if (ticket.WorkshopParticipants != null && ticket.WorkshopParticipants.Count > 0)
                            {
                                _db.WorkshopParticipants.RemoveRange(ticket.WorkshopParticipants);
                            }
                        }

                        _db.WorkshopTickets.RemoveRange(schedule.WorkshopTickets);
                    }
                }

                if (workshop.WorkshopSchedules != null && workshop.WorkshopSchedules.Count > 0)
                {
                    _db.WorkshopSchedules.RemoveRange(workshop.WorkshopSchedules);
                }

                if (workshop.WorkshopImages != null && workshop.WorkshopImages.Count > 0)
                {
                    _db.WorkshopImages.RemoveRange(workshop.WorkshopImages);
                }

                if (workshop.WorkshopReviews != null && workshop.WorkshopReviews.Count > 0)
                {
                    _db.WorkshopReviews.RemoveRange(workshop.WorkshopReviews);
                }

                if (workshop.WorkshopScheduleConfig != null)
                {
                    _db.WorkshopScheduleConfigs.Remove(workshop.WorkshopScheduleConfig);
                }

                if (workshop.Users != null && workshop.Users.Count > 0)
                {
                    workshop.Users.Clear();
                }

                _db.Workshops.Remove(workshop);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<PagedResultDto<Workshop>> GetAllPagedAsync(string? status = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var query = _db.Workshops
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status))
            {
                var trimmedStatus = status.Trim().ToLower();
                query = query.Where(w => w.Status == trimmedStatus);
            }

            query = query
                .Include(w => w.Category)
                .Include(w => w.Level)
                .Include(w => w.WorkshopReviews)
                .Include(w => w.WorkshopImages)
                .Include(w => w.WorkshopSchedules)
                    .ThenInclude(ws => ws.WorkshopTickets)
                        .ThenInclude(wt => wt.WorkshopParticipants)
                .OrderByDescending(w => w.CreatedOn);

            var totalCount = await query.CountAsync(cancellationToken);

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<Workshop>
            {
                Data = data.AsReadOnly(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PagedResultDto<Workshop>> GetByUserIdPagedAsync(int userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var query = _db.Workshops
                .AsNoTracking()
                .Where(w => w.CreatedBy == userId)
                .Include(w => w.Category)
                .Include(w => w.Level)
                .Include(w => w.WorkshopReviews)
                .Include(w => w.WorkshopImages)
                .Include(w => w.WorkshopSchedules)
                    .ThenInclude(ws => ws.WorkshopTickets)
                        .ThenInclude(wt => wt.WorkshopParticipants)
                .OrderByDescending(w => w.CreatedOn);

            var totalCount = await query.CountAsync(cancellationToken);

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<Workshop>
            {
                Data = data.AsReadOnly(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<Workshop>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _db.Workshops
                .AsNoTracking()
                .Where(w => w.CreatedBy == userId)
                .Include(w => w.Category)
                .Include(w => w.Level)
                .Include(w => w.WorkshopReviews)
                .Include(w => w.WorkshopImages)
                .Include(w => w.WorkshopSchedules)
                    .ThenInclude(ws => ws.WorkshopTickets)
                        .ThenInclude(wt => wt.WorkshopParticipants)
                .OrderByDescending(w => w.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<Workshop>> GetRecommendationsAsync(int? userId, CancellationToken cancellationToken = default)
        {
            var user = userId == null ? null : await _db.Users
                .Include(u => u.WorkshopParticipants.Where(wp => wp.Status == "paid"))
                    .ThenInclude(wp => wp.Ticket)
                        .ThenInclude(t => t.WorkshopSchedule)
                            .ThenInclude(ws => ws.Workshop)
                                .ThenInclude(w => w.Category)
                .Include(u => u.WorkshopParticipants.Where(wp => wp.Status == "paid"))
                    .ThenInclude(wp => wp.Ticket)
                        .ThenInclude(t => t.WorkshopSchedule)
                            .ThenInclude(ws => ws.Workshop)
                                .ThenInclude(w => w.Level)
                .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

            IQueryable<Workshop> randomWorkshops = _db.Workshops
                .Where(w => w.Status == "verified"
                    && w.WorkshopSchedules.Count != 0
                    && w.WorkshopSchedules.Any(ws => ws.StartOn > DateOnly.FromDateTime(_timeProvider.Now) && ws.WorkshopTickets.Count != 0))
                .Include(w => w.Category)
                .Include(w => w.Level)
                .Include(w => w.WorkshopReviews!)
                .Include(w => w.WorkshopImages!)
                .Include(w => w.WorkshopSchedules!)
                .OrderBy(r => Guid.NewGuid())
                .Take(4);

            if (user == null || user.WorkshopParticipants == null || user.WorkshopParticipants.Count == 0)
            {
                return await randomWorkshops.ToListAsync(cancellationToken);
            }

            var preferredCategories = user.WorkshopParticipants.Select(wp => wp.Ticket.WorkshopSchedule.Workshop.Category.Name).Distinct().ToList();
            var preferredLevels = user.WorkshopParticipants.Select(wp => wp.Ticket.WorkshopSchedule.Workshop.Level.Name).Distinct().ToList();

            IQueryable<Workshop> recommendations = _db.Workshops
                .Where(w => w.Status == "verified"
                    && w.WorkshopSchedules.Count != 0
                    && w.WorkshopSchedules.Any(ws => ws.StartOn > DateOnly.FromDateTime(_timeProvider.Now) && ws.WorkshopTickets.Count != 0)
                    && (preferredCategories.Contains(w.Category.Name) || preferredLevels.Contains(w.Level.Name))
                    && !w.Users.Any(u => u.Id == userId))
                .Include(w => w.Category)
                .Include(w => w.Level)
                .Include(w => w.WorkshopReviews!)
                .Include(w => w.WorkshopImages!)
                .Include(w => w.WorkshopSchedules!)
                .Take(4);

            if (userId.HasValue && userId.Value > 0)
            {
                recommendations = recommendations.Include(w => w.Users.Where(u => u.Id == userId.Value));
            }

            if (!await recommendations.AnyAsync(cancellationToken))
            {
                return await randomWorkshops.ToListAsync(cancellationToken);
            }

            return await recommendations.ToListAsync(cancellationToken);
        }

        public async Task<PagedResultDto<Workshop>> SearchAsync(
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
                .Include(w => w.WorkshopReviews!)
                .Include(w => w.WorkshopImages!);

            if (userId > 0)
            {
                q = q.Include(w => w.Users.Where(u => u.Id == userId));
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                var trimmed = query.Trim();
                var like = $"%{trimmed}%";
                q = q.Where(w =>
                    EF.Functions.Like(w.Title, like) ||
                    (!string.IsNullOrEmpty(w.Description) && EF.Functions.Like(w.Description!, like)) ||
                    (w.Category != null && EF.Functions.Like(w.Category.Name, like)) ||
                    (w.CreatedByNavigation != null && EF.Functions.Like(w.CreatedByNavigation.Name, like))
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