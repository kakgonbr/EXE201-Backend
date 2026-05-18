using System.IdentityModel.Tokens.Jwt;

namespace EXE201_Backend.Services
{
    public interface IAuthService
    {
        Task<bool> Confirm(string email, string otp);
        Task<JwtSecurityToken?> Login(string email, string password);
        Task<bool> Register(string email, string password);
    }
}