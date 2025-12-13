using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitnessCenterProject.Data;
using FitnessCenterProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers
{
    [ApiController]
    [Route("api/trainers")] // ÖNEMLİ: JS tarafındaki /api/trainers ile birebir aynı

    public class TrainersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/trainers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var trainers = await _context.Trainers
                .Include(t => t.FitnessCenter)
                .Select(t => new
                {
                    id = t.Id,
                    fullName = t.FirstName + " " + t.LastName,
                    specialty = t.Specialty,
                    fitnessCenterName = t.FitnessCenter != null ? t.FitnessCenter.Name : null
                })
                .ToListAsync();

            return Ok(trainers);
        }

        // GET: /api/trainers/by-service/3
        // Belirli bir HİZMET için uygun eğitmenleri döner
        [HttpGet("by-service/{serviceId:int}")]
        public async Task<ActionResult<IEnumerable<object>>> GetByService(int serviceId)
        {
            // TrainerService üzerinden join
            var trainers = await _context.TrainerServices
                .Where(ts => ts.ServiceId == serviceId)
                .Select(ts => ts.Trainer!)
                .Distinct()
                .Include(t => t.FitnessCenter)
                .Select(t => new
                {
                    id = t.Id,
                    fullName = t.FirstName + " " + t.LastName,
                    specialty = t.Specialty,
                    fitnessCenterName = t.FitnessCenter != null ? t.FitnessCenter.Name : null
                })
                .ToListAsync();

            return Ok(trainers);
        }
    }
}
