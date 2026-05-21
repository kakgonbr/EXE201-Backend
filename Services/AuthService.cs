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

        private static readonly ConcurrentDictionary<string, (string Identifier, DateTime SentAt)> _emailOtpStore = new();

        public AuthService(IUserRepository userRepository, ITimeProvider timeProvider, IMailService mailService, IConfigurationService configurationService, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _timeProvider = timeProvider;
            _mailService = mailService;
            _configurationService = configurationService;
            _logger = logger;
        }

        public async Task<bool> Register(string email, string password, CancellationToken cancellationToken = default)
        {
            if (!IsEmailAllowed(email) || !IsPasswordAllowed(password))
            {
                return false;
            }

            var existing = await _userRepository.GetByEmailAsync(email, cancellationToken);

            if (existing != null && existing.Verified)
            {
                return false;
            }

            var now = _timeProvider.Now;
            if (_emailOtpStore.TryGetValue(email, out var entry))
            {
                var resendDelay = TimeSpan.FromSeconds(_configurationService.OTP_RESEND_DELAY_SEC);
                if (now - entry.SentAt < resendDelay)
                {
                    return false;
                }
            }

            string? identifier = await _mailService.SendOtp(email, cancellationToken);

            if (identifier == null)
            {
                return false;
            }

            var sentAt = _timeProvider.Now;
            _emailOtpStore[email] = (identifier, sentAt);
            if (existing != null)
            {
                existing.Verified = false;
                existing.CreatedOn = _timeProvider.Now;
                existing.Role = "user";
                existing.IsActive = false;

                existing.PasswordHash = new PasswordHasher<User>().HashPassword(existing, password);

                await _userRepository.UpdateAsync(existing, cancellationToken);
            }
            else
            {
                var user = new User
                {
                    Email = email,
                    Name = email.Split('@')[0],
                    Verified = false,
                    CreatedOn = _timeProvider.Now,
                    Role = "user",
                    IsActive = false
                };

                await _userRepository.AddAsync(user, cancellationToken);
            }

            return true;
        }

        public async Task<bool> Confirm(string email, string otp, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                return false;
            }

            if (user.Verified)
            {
                return false;
            }

            if (_emailOtpStore.TryGetValue(email, out var entry))
            {
                var sentAt = entry.SentAt;
                var expireSpan = TimeSpan.FromSeconds(_configurationService.OTP_EXPIRE_SEC);
                if (_timeProvider.Now - sentAt > expireSpan)
                {
                    _emailOtpStore.TryRemove(email, out _);
                    return false;
                }

                var identifier = entry.Identifier;
                if (_mailService.IsOtpCorrect(identifier, otp))
                {
                    user.Verified = true;
                    user.IsActive = true;
                    await _userRepository.UpdateAsync(user, cancellationToken);

                    _emailOtpStore.TryRemove(email, out _);

                    return true;
                }
            }
            return false;
        }

        public async Task<JwtSecurityToken?> Login(string email, string password, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
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

        public async Task<bool> ResendOtp(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                return false;
            }

            if (user.Verified)
            {
                return false;
            }

            var now = _timeProvider.Now;
            if (_emailOtpStore.TryGetValue(email, out var entry))
            {
                var resendDelay = TimeSpan.FromSeconds(_configurationService.OTP_RESEND_DELAY_SEC);
                if (now - entry.SentAt < resendDelay)
                {
                    return false;
                }
            }

            string? newIdentifier = await _mailService.SendOtp(email, cancellationToken);
            if (newIdentifier == null)
            {
                return false;
            }

            _emailOtpStore[email] = (newIdentifier, now);
            return true;
        }

        private static bool IsEmailAllowed(string email)
        {
            return MailAddress.TryCreate(email, out _);
        }

        private static bool IsPasswordAllowed(string password)
        {
            return password.Length >= 8;
        }
    }
}