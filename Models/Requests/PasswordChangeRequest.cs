namespace EXE201_Backend.Models.Requests
{
    public class PasswordChangeRequest
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
