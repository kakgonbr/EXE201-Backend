using EXE201_Backend.Models.Requests;
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

        [Authorize(Roles = "host")]
        [HttpPost]
        public async Task<IActionResult> CreateWorkshop([FromBody] CreateWorkshopRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Location))
            {
                return BadRequest("Title and location are required.");
            }

            if (request.CategoryId <= 0 || request.LevelId <= 0)
            {
                return BadRequest("CategoryId and LevelId must be provided and greater than zero.");
            }

            if (request.Schedules == null || !request.Schedules.Any())
            {
                return BadRequest("At least one schedule with tickets is required.");
            }

            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var createdId = await _workshopService.CreateWorkshopAsync(request, userId, cancellationToken);
                return CreatedAtAction(nameof(GetWorkshopById), new { id = createdId }, new { id = createdId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllWorkshops([FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var result = await _workshopService.GetAllWorkshopsAsync(status, page, pageSize, cancellationToken);
            return Ok(result);
        }

        [Authorize(Roles = "host")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyWorkshops([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                return Unauthorized();
            }

            var result = await _workshopService.GetWorkshopsByUserIdAsync(userId, page, pageSize, cancellationToken);
            return Ok(result);
        }

        [Authorize(Roles = "host")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateWorkshop(int id, [FromBody] UpdateWorkshopRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var updated = await _workshopService.UpdateWorkshopAsync(id, request, userId, cancellationToken);
                if (!updated)
                {
                    return NotFound();
                }
                return Ok(new { message = "Workshop updated successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [Authorize(Roles = "host,staff")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteWorkshop(int id, CancellationToken cancellationToken = default)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var deleted = await _workshopService.DeleteWorkshopAsync(id, userId, cancellationToken);
                if (!deleted)
                {
                    return NotFound();
                }
                return Ok(new { message = "Workshop deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [Authorize(Roles = "staff")]
        [HttpPost("{id:int}/verify")]
        public async Task<IActionResult> VerifyWorkshop(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var verified = await _workshopService.VerifyWorkshopAsync(id, cancellationToken);
                if (!verified)
                {
                    return NotFound();
                }

                return Ok(new { message = "Workshop verified successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [Authorize(Roles = "host")]
        [HttpGet("tickets")]
        public async Task<IActionResult> GetTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                return Unauthorized();
            }

            var result = await _workshopService.GetTicketsAsync(userId, page, pageSize, cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = "host")]
        [HttpPost("tickets/checkin")]
        public async Task<IActionResult> CheckInUser([FromBody] TicketCheckinRequest request, CancellationToken cancellationToken = default)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var hostId))
            {
                return Unauthorized();
            }

            try
            {
                var checkedIn = await _workshopService.CheckInParticipantAsync(request.TicketId, request.ParticipantId, hostId, cancellationToken);

                if (!checkedIn)
                {
                    return NotFound();
                }

                return Ok(new { message = "User checked in successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [Authorize(Roles = "host")]
        [HttpGet("tickets/{ticketId:int}/participants")]
        public async Task<IActionResult> GetParticipants(int ticketId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var hostId))
            {
                return Unauthorized();
            }
            try
            {
                var result = await _workshopService.GetParticipantsForTicketAsync(ticketId, hostId, page, pageSize, cancellationToken);
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}