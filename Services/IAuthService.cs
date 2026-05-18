using System.IdentityModel.Tokens.Jwt;

namespace EXE201_Backend.Services
{
    public interface IAuthService
    {
        Task<bool> Confirm(string email, string otp, CancellationToken cancellationToken = default);
        Task<JwtSecurityToken?> Login(string email, string password, CancellationToken cancellationToken = default);
        Task<bool> Register(string email, string password, CancellationToken cancellationToken = default);
    }
}