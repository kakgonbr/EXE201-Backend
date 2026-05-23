namespace EXE201_Backend.Models.Requests
{
    public class CreateWorkshopScheduleRequest
    {
        public string StartOn { get; set; } = null!;
        public IEnumerable<CreateWorkshopTicketRequest>? Tickets { get; set; }
    }
}
