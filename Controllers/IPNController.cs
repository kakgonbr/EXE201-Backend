using EXE201_Backend.Models.Dto;
using EXE201_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IPNController : ControllerBase
    {
        private readonly ILogger<IPNController> _logger;
        private readonly IConfigurationService _configurationService;

        private readonly IPaymentService _paymentService;
        private readonly IScheduleService _scheduleService;
        public IPNController(ILogger<IPNController> logger, IConfigurationService configurationService, IPaymentService paymentService, IScheduleService scheduleService)
        {
            _logger = logger;
            _configurationService = configurationService;
            _paymentService = paymentService;
            _scheduleService = scheduleService;
        }

        [HttpPost]
        public async Task<IActionResult> HandleIPN([FromBody] PaymentWebhookDto paymentWebhook, CancellationToken cancellationToken = default)
        {
            var apiKey = Request.Headers["X-Secret-Key"].FirstOrDefault();

            if (string.IsNullOrEmpty(apiKey))
            {
                return Unauthorized("Missing X-Secret-Key");
            }

            if (apiKey != _configurationService.SE_IPN_SECRET)
            {
                return Unauthorized("Invalid secret");
            }

            string invoiceNum = paymentWebhook.Order.OrderInvoiceNumber;

            string[] invoiceIds = invoiceNum.Split('_');

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

            if (paymentWebhook.Order.OrderStatus == OrderStatus.CAPTURED)
            {
                await _paymentService.InformPaymentStatus(userId, ticketId, paymentWebhook.Order.OrderAmount, cancellationToken);
            }
            else
            {
                await _paymentService.InformPaymentStatus(userId, ticketId, 0, cancellationToken);
            }

            return Ok();
        }

        [Authorize]
        [HttpGet("proceed/{ticketId}")]
        public async Task<IActionResult> ProceedPayment(int ticketId, CancellationToken cancellationToken = default)
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
        public async Task<IActionResult> PaymentStatus(string invoiceNumber, CancellationToken cancellationToken = default)
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
