using EXE201_Backend.Data;
using EXE201_Backend.Models;
using EXE201_Backend.Extensions;
using Microsoft.EntityFrameworkCore;
using EXE201_Backend.Services;

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

        /// <summary>
        /// Search / filter / sort / paginate workshops. All filter parameters are optional (nullable).
        /// Includes related entities useful for listing (price range, schedules, reviews, images) so callers
        /// can compute display fields (min/max price, upcoming schedules, rating, review count) in service/DTO layer.
        /// </summary>
        public async Task<PagedResult<Workshop>> SearchAsync(
            string? query = null,
            IEnumerable<string>? locations = null,
            IEnumerable<int>? categoryIds = null,
            IEnumerable<string>? levels = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int? durationMin = null,
            int? durationMax = null,
            int? scheduleWithinDays = null, // find workshops with schedules within next N days
            WorkshopSort? sortBy = null,
            bool sortDesc = false,
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
                .Include(w => w.WorkshopSchedules!).ThenInclude(s => s.WorkshopTickets!)
                .Include(w => w.WorkshopReviews!)
                .Include(w => w.WorkshopImages!);

            // Adaptive general text search (title, description, instructor, category name)
            if (!string.IsNullOrWhiteSpace(query))
            {
                var trimmed = query.Trim();
                // Use EF.Functions.Like for SQL translation and partial matches
                var like = $"%{trimmed}%";
                q = q.Where(w =>
                    EF.Functions.Like(w.Title, like) ||
                    (!string.IsNullOrEmpty(w.Description) && EF.Functions.Like(w.Description!, like)) ||
                    EF.Functions.Like(w.InstructorName, like) ||
                    (w.Category != null && EF.Functions.Like(w.Category.Name, like))
                );
            }

            // Locations filter (any of provided locations)
            if (locations != null && locations.Any())
            {
                var locList = locations.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
                if (locList.Count != 0)
                {
                    q = q.Where(w => locList.Contains(w.Location));
                }
            }

            // Categories filter
            if (categoryIds != null && categoryIds.Any())
            {
                var catList = categoryIds.ToList();
                q = q.Where(w => catList.Contains(w.CategoryId));
            }

            // Levels filter
            if (levels != null && levels.Any())
            {
                var levelList = levels.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
                if (levelList.Count != 0)
                {
                    q = q.Where(w => levelList.Contains(w.Level));
                }
            }

            // Duration range
            if (durationMin.HasValue)
            {
                q = q.Where(w => w.Duration >= durationMin.Value);
            }
            if (durationMax.HasValue)
            {
                q = q.Where(w => w.Duration <= durationMax.Value);
            }

            // Price range: workshop qualifies if it has at least one ticket within the range
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

            // Schedules within next N days
            if (scheduleWithinDays.HasValue && scheduleWithinDays.Value >= 0)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var upper = today.AddDays(scheduleWithinDays.Value);
                q = q.Where(w =>
                    w.WorkshopSchedules!.Any(s => s.StartOn >= today && s.StartOn <= upper)
                );
            }

            // Sorting
            // If no sort provided, default to newest (CreatedOn desc)
            sortBy ??= WorkshopSort.Relevance;

            // Build ordering expressions. Some aggregates (price, rating) use sub-queries.
            switch (sortBy.Value)
            {
                case WorkshopSort.Name:
                    q = sortDesc ? q.OrderByDescending(w => w.Title) : q.OrderBy(w => w.Title);
                    break;

                case WorkshopSort.Price:
                    // Order by minimum ticket price (workshop's cheapest available ticket). Workshops without tickets will be treated as having price 0.
                    q = sortDesc
                        ? q.OrderByDescending(w => w.WorkshopSchedules!
                                                    .SelectMany(s => s.WorkshopTickets!)
                                                    .Select(t => (decimal?)t.Price)
                                                    .DefaultIfEmpty(0).Min())
                        : q.OrderBy(w => w.WorkshopSchedules!
                                        .SelectMany(s => s.WorkshopTickets!)
                                        .Select(t => (decimal?)t.Price)
                                        .DefaultIfEmpty(0).Min());
                    break;

                case WorkshopSort.Rating:
                    // Order by average rating (out of reviews). Workshops without reviews are treated as 0.
                    q = sortDesc
                        ? q.OrderByDescending(w => w.WorkshopReviews!
                                                    .Select(r => (double?)r.Rating)
                                                    .DefaultIfEmpty(0).Average())
                        : q.OrderBy(w => w.WorkshopReviews!
                                        .Select(r => (double?)r.Rating)
                                        .DefaultIfEmpty(0).Average());
                    break;

                case WorkshopSort.Duration:
                    q = sortDesc ? q.OrderByDescending(w => w.Duration) : q.OrderBy(w => w.Duration);
                    break;

                case WorkshopSort.Level:
                    q = sortDesc ? q.OrderByDescending(w => w.Level) : q.OrderBy(w => w.Level);
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
