namespace EXE201_Backend.Models.Requests
{
    public class PasswordResetRequest
    {
        public string Otp { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
