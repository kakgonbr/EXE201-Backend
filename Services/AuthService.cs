using EXE201_Backend.Models;
using EXE201_Backend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace EXE201_Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IMailService _mailService;
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<AuthService> _logger;
        private static readonly ConcurrentDictionary<string, string> _emailOtpIdentifier = new ConcurrentDictionary<string, string>();

        public AuthService(IUserRepository userRepository, ITimeProvider timeProvider, IMailService mailService, IConfigurationService configurationService, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _timeProvider = timeProvider;
            _mailService = mailService;
            _configurationService = configurationService;
            _logger = logger;
        }

        public async Task<bool> Register(string email, string password)
        {
            if (!IsEmailAllowed(email) || !IsPasswordAllowed(password))
            {
                return false;
            }

            var existing = await _userRepository.GetByEmailAsync(email);

            if (existing != null)
            {
                return false;
            }

            string? identifier = await _mailService.SendOtp(email);

            if (identifier == null || !_emailOtpIdentifier.TryAdd(email, identifier))
            {
                return false;
            }

            var user = new User
            {
                Email = email,
                Name = email.Split('@')[0],
                Verified = false,
                CreatedOn = _timeProvider.Now,
                Role = "user",
                IsActive = false
            };

            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);

            await _userRepository.AddAsync(user);

            return true;
        }

        public async Task<bool> Confirm(string email, string otp)
        {
            if (_emailOtpIdentifier.TryGetValue(email, out var identifier) && _mailService.IsOtpCorrect(identifier, otp))
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user != null)
                {
                    user.Verified = true;
                    user.IsActive = true;
                    await _userRepository.UpdateAsync(user);
                    return true;
                }
            }
            return false;
        }

        public async Task<JwtSecurityToken?> Login(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user != null && user.Verified && user.IsActive)
            {
                var result = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash!, password);
                if (result == PasswordVerificationResult.Success)
                {
                    var claims = new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Role, user.Role)
                        };

                    var key = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configurationService.JWT_KEY));

                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    return new JwtSecurityToken(
                        issuer: _configurationService.JWT_ISSUER,
                        audience: _configurationService.JWT_AUDIENCE,
                        claims: claims,
                        expires: DateTime.UtcNow.AddHours(24),
                        signingCredentials: creds);
                }
            }
            return null;
        }

        private bool IsEmailAllowed(string email)
        {
            return MailAddress.TryCreate(email, out _);
        }

        private bool IsPasswordAllowed(string password)
        {
            return password.Length >= 8;
        }
    }
}