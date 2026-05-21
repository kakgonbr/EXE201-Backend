using EXE201_Backend.Models.Dto;
using EXE201_Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace EXE201_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IPNController : ControllerBase
    {
        private readonly ILogger<IPNController> _logger;
        private readonly IConfigurationService _configurationService;

        private readonly IPaymentService _paymentService;
        public IPNController(ILogger<IPNController> logger, IConfigurationService configurationService, IPaymentService paymentService)
        {
            _logger = logger;
            _configurationService = configurationService;
            _paymentService = paymentService;
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

            return Ok();
        }
    }
}
