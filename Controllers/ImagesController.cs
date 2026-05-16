using EXE201_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class ImagesController : ControllerBase
    {
        private readonly ILogger<ImagesController> _logger;
        private readonly IImageService _imageService;

        public ImagesController(ILogger<ImagesController> logger, IImageService imageService)
        {
            _logger = logger;
            _imageService = imageService;
        }

        [Authorize(Roles = "staff")]
        [HttpPost("staff")]
        public async Task<IActionResult> StaffUpload([FromForm] IFormFile file)
        {
            string status = await _imageService.Upload(file);

            if (status.StartsWith("Failed"))
            {
                return BadRequest(status);
            }

            var url = $"{Request.Scheme}://{Request.Host}/images/{status}";
            return Ok(new { message = "File uploaded.", url, name = status });
        }

        /// <summary>
        /// For customer image uploading.<br></br>
        /// An uploaded image will be tracked and be deleted after 5 minutes.<br></br>
        /// Call the consume method of the image service object to persist the image.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            string? sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (sub == null || !int.TryParse(sub, out var id))
            {
                return Unauthorized("User ID not found in token.");
            }

            string status = await _imageService.Upload(file);

            if (status.StartsWith("Failed"))
            {
                return BadRequest(status);
            }

            _imageService.CheckImagePresent(status, id);

            var url = $"{Request.Scheme}://{Request.Host}/images/{status}";
            return Ok(new { message = "File uploaded.", url, name = status });
        }
    }
}