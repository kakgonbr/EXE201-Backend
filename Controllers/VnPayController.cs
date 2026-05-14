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
            return Ok(VnPayService.GetLink("123.19.192.191, 123.19.192.191", "", 1000000, "vn", VnpConfig.GetRandomNumber(10)));
        }
    }
}
