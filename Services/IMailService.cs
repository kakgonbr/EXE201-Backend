
namespace EXE201_Backend.Services
{
    public interface IMailService
    {
        bool IsOtpCorrect(string identifier, string otp);
        Task<bool> SendEmail(string to, string subject, string body, CancellationToken cancellationToken = default);
        Task SendHostAccepted(string to, CancellationToken cancellationToken = default);
        Task SendHostRegistration(string to, CancellationToken cancellationToken = default);
        Task SendHostRejected(string to, CancellationToken cancellationToken = default);
        Task<string?> SendOtp(string to, CancellationToken cancellationToken = default);
        Task<string?> SendResetPassword(string to, CancellationToken cancellationToken = default);
        Task SendWithdrawRequestApproved(string to, CancellationToken cancellationToken = default);
        Task SendWithdrawRequestReceived(string to, CancellationToken cancellationToken = default);
        Task SendWithdrawRequestRejected(string to, CancellationToken cancellationToken = default);
    }
}