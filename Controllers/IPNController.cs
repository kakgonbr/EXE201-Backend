using EXE201_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace EXE201_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IPNController : ControllerBase
    {
        private readonly ILogger<IPNController> _logger;
        private readonly IConfigurationService _configurationService;

        public IPNController(ILogger<IPNController> logger, IConfigurationService configurationService)
        {
            _logger = logger;
            _configurationService = configurationService;
        }

        [HttpPost]
        public async Task<IActionResult> HandleIPN()
        {
            var apiKey = Request.Headers["X-Secret-Key"].FirstOrDefault();

            if (string.IsNullOrEmpty(apiKey))
            {
                return BadRequest("Missing X-Secret-Key");
            }

            _logger.LogInformation("Received IPN request with API key: {ApiKey}", apiKey);

            Request.EnableBuffering();

            using var reader = new StreamReader(
                Request.Body,
                Encoding.UTF8,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();

            Request.Body.Position = 0;

            _logger.LogInformation("IPN request body: {Body}", body);

            return Ok();
        }
    }
}
