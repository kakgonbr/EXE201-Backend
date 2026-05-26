namespace EXE201_Backend.Models.Dto
{
    public class WorkshopTicketDetailsDto
    {
        public int Id { get; set; }
        public string TicketType { get; set; } = null!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool IsOngoing { get; set; }
        public string WorkshopTitle { get; set; } = null!;
        public string? WorkshopThumbnailLink { get; set; }
        public string WorkshopLocation { get; set; } = null!;
    }
}
