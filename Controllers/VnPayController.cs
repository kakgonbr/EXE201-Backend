using EXE201_Backend.Utils;
using EXE201_Backend.Utils.Services;
using Microsoft.AspNetCore.Mvc;

namespace EXE201_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VnPayController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(VnPayService.GetLink("127.0.0.1", "", 1000000, "vn", VnpConfig.GetRandomNumber(10)));
        }
    }
}
