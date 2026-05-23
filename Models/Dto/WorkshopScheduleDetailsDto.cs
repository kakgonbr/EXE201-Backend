namespace EXE201_Backend.Models.Dto
{
    public class WorkshopScheduleDetailsDto
    {
        public int Id { get; set; }
        public DateOnly StartOn { get; set; }
        public string WorkshopTitle { get; set; } = null!;
        public string? WorkshopThumbnailLink { get; set; }
        public string WorkshopLocation { get; set; } = null!;
        public IEnumerable<WorkshopTicketDto> Tickets { get; set; } = null!;
    }
}