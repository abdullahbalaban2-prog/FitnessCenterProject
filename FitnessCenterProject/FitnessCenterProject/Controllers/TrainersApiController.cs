using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessCenterProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers
{
    [ApiController]
    [Route("api/trainers")]
    public class TrainersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/trainers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var trainers = await _context.Trainers
                .Include(t => t.FitnessCenter)
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
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
        [HttpGet("by-service/{serviceId:int}")]
        public async Task<IActionResult> GetByService(int serviceId)
        {
            var trainers = await _context.TrainerServices
                .Where(ts => ts.ServiceId == serviceId)
                .Select(ts => ts.Trainer!)
                .Distinct()
                .Include(t => t.FitnessCenter)
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
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

        // GET: /api/trainers/available?date=2025-12-12T18:00:00&serviceId=3
        // serviceId opsiyonel (gönderilirse: o hizmeti verebilen + o saatte müsait olan)
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable([FromQuery] DateTime date, [FromQuery] int? serviceId)
        {
            // “date” ile gelen saat aralığı: 1 saatlik slot gibi düşünelim (istersen 30 dk yaparız)
            var start = date;
            var end = date.AddHours(1);

            var day = date.DayOfWeek;
            var startTime = start.TimeOfDay;
            var endTime = end.TimeOfDay;

            // 1) O gün, o saat aralığını kapsayan availability’si olan trainerlar
            var query = _context.TrainerAvailabilities
                .Where(a => a.DayOfWeek == day
                            && a.StartTime <= startTime
                            && a.EndTime >= endTime)
                .Select(a => a.Trainer!)
                .Distinct()
                .AsQueryable();

            // 2) Eğer serviceId geldiyse: TrainerService üzerinden filtrele
            if (serviceId.HasValue && serviceId.Value > 0)
            {
                query = query.Where(t =>
                    _context.TrainerServices.Any(ts => ts.TrainerId == t.Id && ts.ServiceId == serviceId.Value));
            }

            // 3) O saat aralığında çakışan appointment’ı olanları çıkar
            query = query.Where(t =>
                !_context.Appointments.Any(ap =>
                    ap.TrainerId == t.Id &&
                    ap.StartDateTime < end &&
                    ap.EndDateTime > start));

            var result = await query
                .Include(t => t.FitnessCenter)
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .Select(t => new
                {
                    id = t.Id,
                    fullName = t.FirstName + " " + t.LastName,
                    specialty = t.Specialty,
                    fitnessCenterName = t.FitnessCenter != null ? t.FitnessCenter.Name : null
                })
                .ToListAsync();

            return Ok(result);
        }
    }
}
