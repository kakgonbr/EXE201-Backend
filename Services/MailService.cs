using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace EXE201_Backend.Services
{
    public class MailService : IMailService
    {
        private readonly ITimeProvider _timeProvider;
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<AuthService> _logger;
        private static readonly ConcurrentDictionary<string, (string otp, DateTime expiry)> _otpStore = new ConcurrentDictionary<string, (string otp, DateTime expiry)>();

        public MailService(ITimeProvider timeProvider, IConfigurationService configurationService, ILogger<AuthService> logger)
        {
            _timeProvider = timeProvider;
            _configurationService = configurationService;
            _logger = logger;
        }

        public async Task<string?> SendOtp(string to, CancellationToken cancellationToken = default)
        {
            var identifier = CreateRandomString();
            var otp = GenerateOtp();
            var expiry = _timeProvider.Now.AddMinutes(5);

            if (!_otpStore.TryAdd(identifier, (otp, expiry)))
            {
                _logger.LogError("Failed to store OTP for key {Key}", identifier);
                return null;
            }

            if (!await SendEmail(to, "Your OTP Code", $"Your OTP code is {otp}<br></br> This code will expire in 5 minutes.<br></br> You can visit {_configurationService.SELF_SCHEME}://{_configurationService.SELF_HOST}/api/auth/confirm?email={to}&otp={otp} to activate this account.<br></br>If this OTP has expired, you can either register again, or resend it by visiting this link: {_configurationService.SELF_SCHEME}://{_configurationService.SELF_HOST}/api/auth/resend?email={to}", cancellationToken))
            {
                _otpStore.TryRemove(identifier, out _);
                return null;
            }

            return identifier;
        }

        public bool IsOtpCorrect(string identifier, string otp)
        {
            CleanupExpiredOtps();

            if (_otpStore.TryGetValue(identifier, out var entry))
            {
                if (entry.expiry > _timeProvider.Now && entry.otp == otp)
                {
                    _otpStore.TryRemove(identifier, out _);
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> SendEmail(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new MimeMessage();

                message.From.Add(
                    new MailboxAddress(
                        _configurationService.SMTP_FROM,
                        _configurationService.SMTP_EMAIL));

                message.To.Add(MailboxAddress.Parse(to));

                message.Subject = subject;

                message.Body = new TextPart("html")
                {
                    Text = body
                };

                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(
                    _configurationService.SMTP_SERVER,
                    _configurationService.SMTP_PORT,
                    SecureSocketOptions.StartTls, cancellationToken);

                await smtp.AuthenticateAsync(
                    _configurationService.SMTP_EMAIL,
                    _configurationService.SMTP_PASSWORD, cancellationToken);

                await smtp.SendAsync(message, cancellationToken);

                await smtp.DisconnectAsync(true, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogWarning("Failed to send email to {To}: {Message}", to, e.Message);

                return false;
            }

            return true;
        }

        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private string CreateRandomString()
        {
            string ts = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

            Span<byte> bytes = stackalloc byte[4];
            RandomNumberGenerator.Fill(bytes);

            string rand = Convert.ToHexString(bytes);

            return $"{ts}-{rand}";
        }

        private void CleanupExpiredOtps()
        {
            var now = _timeProvider.Now;
            foreach (var key in _otpStore.Keys)
            {
                if (_otpStore.TryGetValue(key, out var entry) && entry.expiry <= now)
                {
                    _otpStore.TryRemove(key, out _);
                }
            }
        }
    }
}
