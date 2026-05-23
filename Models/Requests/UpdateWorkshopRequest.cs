namespace EXE201_Backend.Models.Requests
{
    public class UpdateWorkshopRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? ThumbnailLink { get; set; }
        public int? CategoryId { get; set; }
        public int? LevelId { get; set; }
        public string? Language { get; set; }
        public string? Status { get; set; }

        public List<UpdateWorkshopScheduleRequest>? Schedules { get; set; }
    }

    public class UpdateWorkshopScheduleRequest
    {
        public string StartOn { get; set; } = string.Empty;
        public List<UpdateWorkshopTicketRequest>? Tickets { get; set; }
    }

    public class UpdateWorkshopTicketRequest
    {
        public string TicketType { get; set; } = "standard";
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int MaxTickets { get; set; }
        public decimal Price { get; set; }
    }
}