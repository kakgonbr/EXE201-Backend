namespace EXE201_Backend.Models.Requests
{
    public class WorkshopReviewRequest
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int Rating { get; set; }
    }
}
