using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FitnessCenterProject.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    [Authorize] // giriş şart
    public class AppointmentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/appointments?memberId=xxxx
        [HttpGet]
        public async Task<IActionResult> GetByMember([FromQuery] string memberId)
        {
            if (string.IsNullOrWhiteSpace(memberId))
                return BadRequest("memberId zorunlu.");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && currentUserId != memberId)
                return Forbid();

            var list = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .Where(a => a.MemberId == memberId)
                .OrderByDescending(a => a.StartDateTime)
                .Select(a => new
                {
                    a.Id,
                    a.StartDateTime,
                    a.EndDateTime,
                    a.Price,
                    status = a.Status.ToString(),
                    service = a.Service != null ? a.Service.Name : null,
                    trainer = a.Trainer != null ? (a.Trainer.FirstName + " " + a.Trainer.LastName) : null
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}
