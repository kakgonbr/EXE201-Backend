namespace EXE201_Backend.Models.Dto
{
    public class WorkshopDisplayDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? ThumbnailLink { get; set; }
        public string? Description { get; set; } = null!;
        public string Language { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Status { get; set; } = string.Empty;
        public string Category { get; set; } = null!;
        public string Status { get; set; } = string.Empty;
        public DateOnly NextSchedule { get; set; }
        public int Duration { get; set; }
        public string Level { get; set; } = null!;
        public decimal PriceLower { get; set; }
        public decimal PriceUpper { get; set; }
        public int MaxTickets { get; set; }
        public int SoldTickets { get; set; }
        public int RemainingTickets { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool Liked { get; set; }
    }
}