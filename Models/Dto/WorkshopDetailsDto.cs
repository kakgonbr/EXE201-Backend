namespace EXE201_Backend.Models.Dto
{
    public class WorkshopDetailsDto
    {
        public string Title { get; set; } = null!;
        public string? ThumbnailLink { get; set; }
        public string? Description { get; set; } = null!;
        public string Language { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string InstructorName { get; set; } = null!;
        public string? InstructorImgLink { get; set; }
        public int Duration { get; set; }
        public string Level { get; set; } = null!;
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool Liked { get; set; }
        public IEnumerable<string> Images { get; set; } = null!;
        public IEnumerable<WorkshopScheduleDto> Schedules { get; set; } = null!;
    }
}
