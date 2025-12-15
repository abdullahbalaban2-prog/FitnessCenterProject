using System.Linq;
using System.Threading.Tasks;
using FitnessCenterProject.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers.Api
{
    [ApiController]
    [Route("api/trainers")]
    [Authorize]
    public class TrainersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public TrainersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("by-service/{serviceId:int}")]
        public async Task<IActionResult> GetByService(int serviceId)
        {
            var list = await _context.TrainerServices
                .Where(ts => ts.ServiceId == serviceId)
                .Select(ts => ts.Trainer!)
                .Distinct()
                .OrderBy(t => t.FirstName)
                .ThenBy(t => t.LastName)
                .Select(t => new
                {
                    id = t.Id,
                    fullName = t.FirstName + " " + t.LastName,
                    specialty = t.Specialty
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}
