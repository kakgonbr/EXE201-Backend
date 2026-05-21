using EXE201_Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EXE201_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkshopsController : ControllerBase
    {
        private readonly ExeContext _context;

        public WorkshopsController(ExeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetWorkshops([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 12;

            var workshops = await _context.Workshops
                .Where(w => w.Status == "active")
                .Include(w => w.Category)
                .OrderByDescending(w => w.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new
                {
                    w.Id,
                    w.Title,
                    w.Location,
                    Price = w.Price,
                    Category = w.Category != null ? w.Category.Name : null,
                    w.ThumbnailLink,
                    w.InstructorName,
                    w.InstructorImgLink,
                    w.Rating,
                    w.Status
                })
                .ToListAsync();

            return Ok(workshops);
        }
    }
}
