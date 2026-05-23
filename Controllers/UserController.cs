using EXE201_Backend.Models;
using EXE201_Backend.Models.Requests;
using EXE201_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userService.GetUser(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetUsers(page, pageSize);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("change-name")]
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeUserInfoRequest changeUserInfoRequest)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(changeUserInfoRequest.NewName))
            {
                return BadRequest();
            }

            var success = await _userService.ChangeUsername(parsedUserId, changeUserInfoRequest.NewName);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [Authorize]
        [HttpPost("change-phone")]
        public async Task<IActionResult> ChangePhoneNumber([FromBody] ChangeUserInfoRequest changeUserInfoRequest)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(changeUserInfoRequest.NewPhoneNumber))
            {
                return BadRequest();
            }

            var success = await _userService.ChangePhonenumber(parsedUserId, changeUserInfoRequest.NewPhoneNumber);
            if (!success)
            {
                return BadRequest();
            }

            return NoContent();
        }

        [Authorize]
        [HttpPost("change-avatar")]
        public async Task<IActionResult> ChangeAvatar([FromBody] ChangeUserInfoRequest changeUserInfoRequest)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(changeUserInfoRequest.NewAvatarUrl))
            {
                return BadRequest();
            }
            
            var success = await _userService.ChangeAvatar(parsedUserId, changeUserInfoRequest.NewAvatarUrl);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
