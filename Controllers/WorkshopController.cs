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

        public WorkshopController(IWorkshopService workshopService)
        {
            _workshopService = workshopService;
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
        [HttpGet("{id:int}")]
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

        [Authorize(Roles = "staff")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllWorkshops(
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _workshopService.GetAllWorkshopsAsync(status, page, pageSize, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    detail = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
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
