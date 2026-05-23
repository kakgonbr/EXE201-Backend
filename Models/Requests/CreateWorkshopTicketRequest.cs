namespace EXE201_Backend.Models.Requests
{
    public class CreateWorkshopTicketRequest
    {
        public string TicketType { get; set; } = null!;
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
        public int MaxTickets { get; set; }
        public decimal Price { get; set; }
    }
}