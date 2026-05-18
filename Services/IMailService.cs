
namespace EXE201_Backend.Services
{
    public interface IMailService
    {
        bool IsOtpCorrect(string identifier, string otp);
        Task<bool> SendEmail(string to, string subject, string body);
        Task<string?> SendOtp(string to);
    }
}