using EXE201_Backend.Models.Requests;
using EXE201_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EXE201_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register also triggers otp resend, same delay and constraints apply, but also resets the password.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email and password are required.");
            }
            bool success = await _authService.Register(request.Email, request.Password, cancellationToken);
            if (success)
            {
                return Ok("Registration successful. Please check your email for the OTP.");
            }
            else
            {
                return BadRequest("Registration failed. Please ensure your email and password meet the requirements.");
            }
        }

        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmOtp([FromQuery] string email, [FromQuery] string otp, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(otp) || string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid OTP.");
            }
            bool success = await _authService.Confirm(email, otp, cancellationToken);
            if (success)
            {
                return Ok("OTP confirmed. Your account is now active, you may login.");
            }
            else
            {
                return BadRequest("OTP confirmation failed. Please ensure you entered the correct OTP and it has not expired.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email and password are required.");
            }
            var token = await _authService.Login(request.Email, request.Password, cancellationToken);
            if (token != null)
            {
                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }
            else
            {
                return Unauthorized("Login failed. Please check your email and password.");
            }
        }

        [HttpGet("resend")]
        public async Task<IActionResult> ResendOtp([FromQuery] string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required.");
            }

            bool success = await _authService.ResendOtp(email, cancellationToken);

            if (success)
            {
                return Ok("OTP resent. Please check your email.");
            }
            else
            {
                return BadRequest("OTP resend failed. Please ensure you are not requesting OTP too frequently and that the email is correct.");
            }
        }

        [HttpGet("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required.");
            }

            bool success = await _authService.ResetPassword(email, cancellationToken);

            if (success)
            {
                return Ok("Password reset email sent. Please check your email.");
            }
            else
            {
                return BadRequest("Password reset failed. Please ensure the email is correct.");
            }
        }

        [HttpPost("reset-password/confirm")]
        public async Task<IActionResult> ConfirmResetPassword([FromBody] PasswordResetRequest passwordResetRequest, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(passwordResetRequest.Email) || string.IsNullOrEmpty(passwordResetRequest.Otp) || string.IsNullOrEmpty(passwordResetRequest.NewPassword))
            {
                return BadRequest("Email, OTP and new password are required.");
            }

            bool success = await _authService.ConfirmPasswordReset(passwordResetRequest.Email, passwordResetRequest.Otp, passwordResetRequest.NewPassword, cancellationToken);

            if (success)
            {
                return Ok("Password reset successful. You may now login with your new password.");
            }
            else
            {
                return BadRequest("Password reset failed. Please ensure you entered the correct OTP and it has not expired.");
            }
        }

        [Authorize]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeRequest passwordChangeRequest, CancellationToken cancellationToken = default)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                return Unauthorized("Invalid user ID in token.");
            }

            if (string.IsNullOrEmpty(passwordChangeRequest.NewPassword) || string.IsNullOrEmpty(passwordChangeRequest.CurrentPassword))
            {
                return BadRequest("Current and new passwords are required.");
            }

            bool success = await _authService.ChangePassword(parsedUserId, passwordChangeRequest.CurrentPassword, passwordChangeRequest.NewPassword, cancellationToken);
            
            if (success)
            {
                return Ok("Password change successful. You may now login with your new password.");
            }
            else
            {
                return BadRequest("Password change failed. Please ensure the email is correct.");
            }
        }
    }
}