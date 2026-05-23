namespace EXE201_Backend.Models.Requests
{
    public class CreateWorkshopRequest
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Location { get; set; } = null!;
        public string? ThumbnailLink { get; set; }
        public int CategoryId { get; set; }
        public int LevelId { get; set; }
        public string Language { get; set; } = "en";
        public IEnumerable<CreateWorkshopScheduleRequest>? Schedules { get; set; }
    }
}