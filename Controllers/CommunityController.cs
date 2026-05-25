using EXE201_Backend.Models.Requests;
using EXE201_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommunityController : ControllerBase
    {
        private readonly ICommunityService _communityService;

        public CommunityController(ICommunityService communityService)
        {
            _communityService = communityService;
        }

        [Authorize(Roles = "host")]
        [HttpPost("reviews/{reviewId}/respond")]
        public async Task<IActionResult> PostWorkshopReviewResponse([FromBody] WorkshopReviewResponseRequest responseRequest, [FromRoute] int reviewId, CancellationToken cancellationToken = default)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                return Unauthorized();
            }

            var result = await _communityService.PostWorkshopReviewResponseAsync(parsedUserId, reviewId, responseRequest.Response);

            if (!result)
            {
                return BadRequest();
            }

            return Ok();
        }

        [Authorize]
        [HttpPost("reviews/{workshopId}")]
        public async Task<IActionResult> PostWorkshopReview([FromBody] WorkshopReviewRequest reviewRequest, [FromRoute] int workshopId, CancellationToken cancellationToken = default)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                return Unauthorized();
            }
            var result = await _communityService.PostWorkshopReviewAsync(parsedUserId, workshopId, reviewRequest.Title, reviewRequest.Description, reviewRequest.Rating, cancellationToken);
            if (!result)
            {
                return BadRequest();
            }
            return Ok();
        }

        [HttpGet("reviews/{workshopId}")]
        public async Task<IActionResult> GetWorkshopReviews([FromRoute] int workshopId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var reviews = await _communityService.GetWorkshopReviewsAsync(workshopId, page, pageSize, cancellationToken);

            if (reviews == null)
            {
                return NotFound();
            }

            return Ok(reviews);
        }

        [Authorize(Roles = "host")]
        [HttpGet("reviews")]
        public async Task<IActionResult> GetAllReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                return Unauthorized();
            }

            var reviews = await _communityService.GetHostReviewsAsync(parsedUserId, page, pageSize, cancellationToken);
            if (reviews == null)
            {
                return NotFound();
            }

            return Ok(reviews);
        }

        [Authorize]
        [HttpPost("like/{workshopId}")] // toggle
        public async Task<IActionResult> LikeWorkshop([FromRoute] int workshopId, CancellationToken cancellationToken = default)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                return Unauthorized();
            }

            var result = await _communityService.ToggleLikeWorkshopAsync(parsedUserId, workshopId, cancellationToken);
            if (!result)
            {
                return BadRequest();
            }

            return Ok();
        }
    }
}