namespace EXE201_Backend.Models.Dto
{
    public class WorkshopParticipantDto
    {
        public int ParticipantId { get; set; }
        public int TicketId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime BookedOn { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string? AvatarLink { get; set; }
    }
}
