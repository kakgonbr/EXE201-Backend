using EXE201_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetScheduleDetails(int id, CancellationToken cancellationToken = default)
        {
            var result = await _scheduleService.GetScheduleDetailsAsync(id, cancellationToken);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [Authorize]
        [HttpGet("month/{month}")]
        public async Task<IActionResult> GetSchedulesForMonth(int month, CancellationToken cancellationToken = default)
        {
            int userId;
            if (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                userId = parsedUserId;
            }
            else
            {
                return Unauthorized();
            }

            return Ok(await _scheduleService.GetSchedulesInMonthAsync(userId, month, cancellationToken));
        }

        [Authorize]
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingSchedules([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            int userId;
            if (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                userId = parsedUserId;
            }
            else
            {
                return Unauthorized();
            }

            var result = await _scheduleService.GetUpcoming(userId, page, pageSize, cancellationToken);
            return Ok(result);
        }
    }
}
