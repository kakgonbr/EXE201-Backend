using EXE201_Backend.Models;
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
    public class HostController : ControllerBase
    {
        private readonly IHostService _hostService;
        public HostController(IHostService hostService)
        {
            _hostService = hostService;
        }

        [Authorize]
        [HttpPost("verification")]
        public async Task<IActionResult> HostVerification(CancellationToken cancellationToken = default)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                return Unauthorized();
            }

            var result = await _hostService.RegisterHostAsync(parsedUserId, cancellationToken);

            if (result)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Failed to register host.");
            }
        }

        [Authorize(Roles = "staff")]
        [HttpPost("update")]
        public async Task<IActionResult> UpdateHostRegistration([FromBody] HostRegistrationUpdateRequest hostRegistrationUpdateRequest, CancellationToken cancellationToken = default)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                return Unauthorized();
            }

            var result = await _hostService.UpdateHostRegistrationAsync(parsedUserId, hostRegistrationUpdateRequest.HostId, hostRegistrationUpdateRequest.Approved, hostRegistrationUpdateRequest.Note, cancellationToken);

            if (result)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Failed to update host registration.");
            }
        }

        [Authorize(Roles = "staff")]
        [HttpGet("registrations")]
        public async Task<IActionResult> GetHostRegistrations(ApproveStatusFilter? approveFilter = null,
            HostRegistrationSort? sortBy = null,
            bool sortDesc = false, int page = 1,
            int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _hostService.GetHostRegistrations(approveFilter, sortBy, sortDesc, page, pageSize, cancellationToken);
            return Ok(result);
        }
    }
}
