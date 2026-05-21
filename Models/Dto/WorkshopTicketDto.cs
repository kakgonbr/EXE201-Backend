namespace EXE201_Backend.Models.Dto
{
    public class WorkshopTicketDto
    {
        public int Id { get; set; }

        public string TicketType { get; set; } = null!;

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }
        public int RemainingTickets { get; set; }
        public decimal Price { get; set; }
    }
}
