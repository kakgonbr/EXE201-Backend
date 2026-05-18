using EXE201_Backend.Models.Requests;
using EXE201_Backend.Services;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetWorkshops([FromQuery] WorkshopQuery query, CancellationToken cancellationToken)
        {
            var result = await _workshopService.GetWorkshopAsync(
                query.SearchTerm,
                query.Locations,
                query.CategoryIds,
                query.Levels,
                query.PriceMin,
                query.PriceMax,
                query.DurationMin,
                query.DurationMax,
                query.ScheduleWithinDays,
                query.SortBy,
                query.SortDesc,
                query.Page,
                query.PageSize,
                cancellationToken
            );

            return Ok(result);
        }
    }
}
