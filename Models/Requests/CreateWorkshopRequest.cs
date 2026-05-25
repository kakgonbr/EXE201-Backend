namespace EXE201_Backend.Models.Requests
{
    public class CreateWorkshopRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? ThumbnailLink { get; set; }
        public IEnumerable<string>? ImageLinks { get; set; }
        public int CategoryId { get; set; }
        public int LevelId { get; set; }
        public string Language { get; set; } = "en";
        public string Status { get; set; } = "pending";
        public IEnumerable<CreateWorkshopScheduleRequest>? Schedules { get; set; }
    }
}