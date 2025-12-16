using System;
using System.Collections.Generic;
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

        // ---------------------------------------------------------------
        // 1) Hizmete göre eğitmenleri getir
        // ---------------------------------------------------------------
        [HttpGet("by-service/{serviceId:int}")]
        public async Task<IActionResult> GetByService(int serviceId)
        {
            var trainers = await _context.TrainerServices
                .Where(ts => ts.ServiceId == serviceId)
                .Select(ts => ts.Trainer!)
                .OrderBy(t => t.FirstName)
                .Select(t => new
                {
                    id = t.Id,
                    fullName = t.FirstName + " " + t.LastName
                })
                .ToListAsync();

            return Ok(trainers);
        }

        // ---------------------------------------------------------------
        // 2) Eğitmenin bir gündeki boş saatlerini getir (MHRS mantığı)
        // Parametreler:
        //  - trainerId
        //  - date: "yyyy-MM-dd"
        //  - duration: dakika (isteğe bağlı; verilmezse hizmetin süresi kullanılmalı)
        //  - serviceId: duration yoksa zorunlu
        // ---------------------------------------------------------------
        [HttpGet("free-slots")]
        public async Task<IActionResult> GetFreeSlots(
            int trainerId,
            string date,
            int? duration,
            int? serviceId)
        {
            if (!DateTime.TryParse(date, out var selectedDate))
                return BadRequest("Invalid date.");

            // Eğitmenin o gün çalışma aralığı
            var availability = await _context.TrainerAvailabilities
                .FirstOrDefaultAsync(a => a.TrainerId == trainerId && a.DayOfWeek == selectedDate.DayOfWeek);

            if (availability == null)
                return Ok(new List<string>()); // O gün çalışmıyor

            int durMin = duration ?? 0;
            if (durMin <= 0)
            {
                if (serviceId == null) return BadRequest("duration or serviceId required.");
                var svc = await _context.Services.FindAsync(serviceId.Value);
                if (svc == null) return BadRequest("Service not found.");
                durMin = svc.DurationMinutes;
            }

            var startSpan = availability.StartTime;
            var endSpan = availability.EndTime;
            var step = TimeSpan.FromMinutes(durMin);

            // Sadece ONAYLI randevular slotları kapatsın
            var approved = await _context.Appointments
                .Where(a => a.TrainerId == trainerId
                            && a.Status == Models.AppointmentStatus.Approved
                            && a.StartDateTime.Date == selectedDate.Date)
                .Select(a => new { a.StartDateTime, a.EndDateTime })
                .ToListAsync();

            var result = new List<string>();

            for (var t = startSpan; t.Add(step) <= endSpan; t = t.Add(step))
            {
                var slotStart = selectedDate.Date + t;      // yerel
                var slotEnd = slotStart.Add(step);

                bool clash = approved.Any(a =>
                    slotStart < a.EndDateTime && slotEnd > a.StartDateTime);

                if (!clash)
                    result.Add(slotStart.ToString("yyyy-MM-ddTHH:mm")); // Z YOK
            }

            return Ok(result);
        }

        // ---------------------------------------------------------------
        // 3) Geriye dönük uyumluluk: /api/trainers/slots ... (alias)
        // ---------------------------------------------------------------
        [HttpGet("slots")]
        public Task<IActionResult> GetSlotsAlias(
            int trainerId, string date, int duration, int? serviceId = null)
            => GetFreeSlots(trainerId, date, duration, serviceId);
    }
}
