namespace EXE201_Backend.Models.Requests
{
    public class UpdateWorkshopRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? ThumbnailLink { get; set; }
        public IEnumerable<string>? ImageLinks { get; set; }
        public int? CategoryId { get; set; }
        public int? LevelId { get; set; }
        public string? Language { get; set; }

        public List<UpdateWorkshopScheduleRequest>? Schedules { get; set; }
    }

    public class UpdateWorkshopScheduleRequest
    {
        public string? StartOn { get; set; }
        public List<UpdateWorkshopTicketRequest>? Tickets { get; set; }
    }

    public class UpdateWorkshopTicketRequest
    {
        public string? TicketType { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public int? MaxTickets { get; set; }
        public decimal? Price { get; set; }
    }
}