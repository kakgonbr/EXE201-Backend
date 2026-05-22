using System.IdentityModel.Tokens.Jwt;

namespace EXE201_Backend.Services
{
    public interface IAuthService
    {
        Task<bool> ChangePassword(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
        Task<bool> Confirm(string email, string otp, CancellationToken cancellationToken = default);
        Task<bool> ConfirmPasswordReset(string email, string otp, string newPassword, CancellationToken cancellationToken = default);
        Task<JwtSecurityToken?> Login(string email, string password, CancellationToken cancellationToken = default);
        Task<bool> Register(string email, string password, CancellationToken cancellationToken = default);
        Task<bool> ResendOtp(string email, CancellationToken cancellationToken = default);
        Task<bool> ResetPassword(string email, CancellationToken cancellationToken = default);
    }
}