namespace EXE201_Backend.Models.Dto
{
    public class WorkshopDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? ThumbnailLink { get; set; }
        public string? Description { get; set; } = null!;
        public string Language { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Status { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string Level { get; set; } = null!;
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool Liked { get; set; }
        public IEnumerable<string> Images { get; set; } = null!;
        public IEnumerable<WorkshopScheduleDetailsDto> Schedules { get; set; } = null!;
    }
}