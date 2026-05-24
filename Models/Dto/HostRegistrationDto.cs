namespace EXE201_Backend.Models.Dto
{
    public class HostRegistrationDto
    {
        public int UserId { get; set; }
        public bool Approved { get; set; }
        public string? Note { get; set; }
        public string UserName { get; set; } = default!;
        public string? ApprovedBy { get; set; }
        public string? UserAvatarUrl { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
