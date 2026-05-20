namespace EXE201_Backend.Models.Dto
{
    public class WorkshopScheduleDto
    {
        public int Id { get; set; }
        public DateOnly StartOn { get; set; }
        public int RemainingTickets { get; set; }
        public decimal PriceLower { get; set; }
        public decimal PriceUpper { get; set; }
    }
}
