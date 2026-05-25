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
    public class RevenueController : ControllerBase
    {
        private readonly IRevenueService _revenueService;

        public RevenueController(IRevenueService revenueService)
        {
            _revenueService = revenueService;
        }

        [Authorize(Roles = "staff")]
        [HttpGet("requests")]
        public async Task<IActionResult> GetHostWithdrawRequests(
            [FromQuery] WithdrawStatusFilter? statusFilter = null,
            [FromQuery] HostWithdrawSort? sortBy = null,
            [FromQuery] bool sortDesc = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var requests = await _revenueService.GetWithdrawRequestsAsync(statusFilter, sortBy, sortDesc, page, pageSize, cancellationToken);
            return Ok(requests);
        }

        [Authorize(Roles = "staff")]
        [HttpPost("requests/update")]
        public async Task<IActionResult> UpdateHostWithdrawRequest([FromBody] HostWithdrawUpdateRequest hostWithdrawUpdateRequest, CancellationToken cancellationToken = default)
        {
            var result = await _revenueService.UpdateWithdrawRequestAsync(hostWithdrawUpdateRequest.Id, hostWithdrawUpdateRequest.Status, hostWithdrawUpdateRequest.Note, cancellationToken);

            if (result)
            {
                return Ok(new { message = "Withdraw request updated successfully." });
            }
            return BadRequest(new { message = "Failed to update withdraw request." });
        }

        [HttpPost("requests")]
        [Authorize(Roles = "host")]
        public async Task<IActionResult> CreateHostWithdrawRequest([FromBody] HostWithdrawCreateRequest hostWithdrawCreateRequest, CancellationToken cancellationToken = default)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                return Unauthorized();
            }

            var result = await _revenueService.CreateWithdrawRequestAsync(parsedUserId, hostWithdrawCreateRequest.Amount, hostWithdrawCreateRequest.BankName, hostWithdrawCreateRequest.BankAccount, cancellationToken);

            if (result)
            {
                return Ok(new { message = "Withdraw request created successfully." });
            }
            return BadRequest(new { message = "Failed to create withdraw request." });
        }

        [HttpGet("statistics")]
        [Authorize(Roles = "host")]
        public async Task<IActionResult> GetRevenueStatistics([FromQuery] int year, CancellationToken cancellationToken = default)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
            {
                return Unauthorized();
            }

            var statistics = await _revenueService.GetRevenueStatisticsAsync(parsedUserId, cancellationToken);

            return Ok(statistics);
        }
    }
}