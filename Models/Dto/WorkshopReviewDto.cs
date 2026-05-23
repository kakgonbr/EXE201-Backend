namespace EXE201_Backend.Models.Dto
{
    public class WorkshopReviewDto
    {
        public int Id { get; set; }
        public string ReviewerName { get; set; } = null!;
        public int ReviewerId { get; set; }
        public string ReviewerAvatarLink { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; } = null!;
        public string? Response { get; set; } = null!;
        public int Rating { get; set; }
        public string? WorkshopName { get; set; } = null!;
        public int WorkshopId { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
