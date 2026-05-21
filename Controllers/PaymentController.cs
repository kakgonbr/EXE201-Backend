using EXE201_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IPaymentService _paymentService;
        private readonly IScheduleService _scheduleService;

        public PaymentController(
            ILogger<PaymentController> logger,
            IPaymentService paymentService,
            IScheduleService scheduleService)
        {
            _logger = logger;
            _paymentService = paymentService;
            _scheduleService = scheduleService;
        }

        [Authorize]
        [HttpGet("proceed/{ticketId}")]
        public async Task<IActionResult> ProceedPayment(int ticketId, CancellationToken cancellationToken)
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

            if (await _scheduleService.IsUserOccupiedAsync(userId, ticketId, cancellationToken))
            {
                return BadRequest("User is currently occupied with another schedule.");
            }

            var result = await _paymentService.StartCheckout(userId, ticketId, cancellationToken);

            if (result == null)
            {
                return BadRequest("Unable to start payment process.");
            }

            return Ok(result);
        }


        [HttpGet("status/{invoiceNumber}")]
        public async Task<IActionResult> PaymentStatus(string invoiceNumber, CancellationToken cancellationToken)
        {
            string[] invoiceIds = invoiceNumber.Split('_');

            if (invoiceIds.Length != 4)
            {
                return BadRequest("Invalid invoice number format");
            }

            if (!int.TryParse(invoiceIds[1], out int userId))
            {
                return BadRequest("Invalid user ID in invoice number");
            }

            if (!int.TryParse(invoiceIds[2], out int ticketId))
            {
                return BadRequest("Invalid ticket ID in invoice number");
            }

            var status = await _paymentService.GetPaymentStatusAsync(userId, ticketId, cancellationToken);

            if (status == null)
            {
                return NotFound();
            }

            return Ok(new { status });
        }
    }
}