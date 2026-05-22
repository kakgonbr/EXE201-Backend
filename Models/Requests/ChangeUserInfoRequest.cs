namespace EXE201_Backend.Models.Requests
{
    public class ChangeUserInfoRequest
    {
        public string? NewName { get; set; }
        public string? NewPhoneNumber { get; set; }
        public string? NewAvatarUrl { get; set; }
        public string? NewLocation { get; set; }
    }
}
