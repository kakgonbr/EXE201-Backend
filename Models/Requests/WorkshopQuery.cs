using EXE201_Backend.Repositories;

namespace EXE201_Backend.Models.Requests
{
    public class WorkshopQuery
    {
        internal int? DurationMin;
        public string? SearchTerm { get; internal set; }
        public IEnumerable<string>? Locations { get; internal set; }
        public IEnumerable<int>? CategoryIds { get; internal set; }
        public IEnumerable<string>? Levels { get; internal set; }
        public decimal? PriceMin { get; internal set; }
        public decimal? PriceMax { get; internal set; }
        public int? DurationMax { get; internal set; }
        public int? ScheduleWithinDays { get; internal set; }
        public WorkshopSort? SortBy { get; internal set; }
        public bool SortDesc { get; internal set; }
        public int Page { get; internal set; } = 1;
        public int PageSize { get; internal set; } = 10;
    }
}
