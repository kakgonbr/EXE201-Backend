using EXE201_Backend.Models.Requests;
using EXE201_Backend.Repositories;
using EXE201_Backend.Services;
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
