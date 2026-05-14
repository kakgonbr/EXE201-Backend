using EXE201_Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EXE201_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IPNController : ControllerBase
    {
        private readonly ILogger<IPNController> _logger;

        public IPNController(ILogger<IPNController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation("IPN Received a GET request from {ip}", HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString());

            var queryParams = HttpContext.Request.Query;
            var fields = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var param in queryParams)
            {
                var key = WebUtility.UrlEncode(param.Key);
                var value = WebUtility.UrlEncode(param.Value);

                if (!string.IsNullOrEmpty(value))
                {
                    fields[key] = value;
                }
            }

            string? secureHash = queryParams["vnp_SecureHash"];
            fields.Remove("vnp_SecureHashType");
            fields.Remove("vnp_SecureHash");

            string? txnRef = queryParams["vnp_TxnRef"];
            string? amountStr = queryParams["vnp_Amount"];
            string? payDate = queryParams["vnp_PayDate"];
            string? responseCode = queryParams["vnp_ResponseCode"];

            if (txnRef is null)
                return BadRequest("No order id");

            if (!long.TryParse(amountStr, out long rawAmount))
                return BadRequest("Invalid amount");

            var paidAmount = rawAmount / 100.0m;

            string calculatedHash = VnpConfig.HashAllFields(fields);

            bool finalPayment = txnRef.StartsWith('f');

            _logger.LogInformation("txnref: {Ref}", txnRef);

            bool success = false;
            if (string.Equals(calculatedHash, secureHash, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Order {OrderId}, amount {Amount}, date {Date}", txnRef, paidAmount, payDate);

                if (responseCode == "00")
                {
                    _logger.LogInformation("Payment successful.");
                }
                else
                {
                    _logger.LogInformation("Response code is not 00");
                }
            }
            else
            {
                _logger.LogInformation("Hash doesnt match");
            }

            return Ok();
        }
    }
}
