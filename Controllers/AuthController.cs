using EXE201_Backend.Models.Requests;
using EXE201_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

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
    }
}
