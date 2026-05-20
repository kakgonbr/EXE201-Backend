using EXE201_Backend.Repositories;
using EXE201_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkshopController : ControllerBase
    {
        private readonly IWorkshopService _workshopService;
        private readonly IScheduleService _scheduleService;

        public WorkshopController(IWorkshopService workshopService, IScheduleService scheduleService)
        {
            _workshopService = workshopService;
            _scheduleService = scheduleService;
        }

        [Authorize]
        [AllowAnonymous]
        [HttpGet("recommendations")]
        public async Task<IActionResult> GetRecommendedWorkshops(CancellationToken cancellationToken = default)
        {
            int? userId;
            if (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                userId = parsedUserId;
            }
            else
            {
                userId = null;
            }
            var result = await _workshopService.GetRecommendedWorkshopsAsync(userId, cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkshopById(int id, CancellationToken cancellationToken = default)
        {
            int? userId;
            if (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                userId = parsedUserId;
            }
            else
            {
                userId = null;
            }
            var result = await _workshopService.GetWorkshopByIdAsync(id, userId, cancellationToken);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("schedule/{id}")]
        public async Task<IActionResult> GetScheduleDetails(int id, CancellationToken cancellationToken = default)
        {
            var result = await _scheduleService.GetScheduleDetailsAsync(id, cancellationToken);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetWorkshops(
            [FromQuery] string? query,
            [FromQuery] IEnumerable<string>? locations,
            [FromQuery] IEnumerable<string>? categories,
            [FromQuery] IEnumerable<string>? levels,
            [FromQuery] decimal? priceMin,
            [FromQuery] decimal? priceMax,
            [FromQuery] int? durationMin,
            [FromQuery] int? durationMax,
            [FromQuery] int? scheduleWithinDays,
            [FromQuery] WorkshopSort? sortBy,
            [FromQuery] bool sortDesc,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            int userId;

            if (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                userId = parsedUserId;
            }
            else
            {
                userId = 0;
            }

            var result = await _workshopService.GetWorkshopAsync(
                query,
                locations,
                categories,
                levels,
                priceMin,
                priceMax,
                durationMin,
                durationMax,
                scheduleWithinDays,
                sortBy,
                sortDesc,
                userId,
                page,
                pageSize,
                cancellationToken
            );

            return Ok(result);
        }
    }
}
