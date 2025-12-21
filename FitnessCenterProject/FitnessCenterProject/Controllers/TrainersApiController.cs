using System;
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
    [Route("api/trainers")]
    public class TrainersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1) Hizmete göre eğitmenleri getir
        // GET: /api/trainers/by-service/5
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

        // 2) Eğitmenin bir gündeki boş saatlerini getir (MHRS mantığı)
        // GET: /api/trainers/free-slots?trainerId=1&date=2025-12-27&duration=60
        // Not: SADECE Approved randevular slot kapatır.
        [HttpGet("free-slots")]
        public async Task<IActionResult> GetFreeSlots(
            int trainerId,
            string date,
            int? duration,
            int? serviceId)
        {
            if (!DateTime.TryParse(date, out var selectedDate))
                return BadRequest("Invalid date.");

            int durMin = duration ?? 0;
            if (durMin <= 0)
            {
                if (serviceId == null) return BadRequest("duration or serviceId required.");
                var svc = await _context.Services.FindAsync(serviceId.Value);
                if (svc == null) return BadRequest("Service not found.");
                durMin = svc.DurationMinutes;
            }

            var step = TimeSpan.FromMinutes(durMin);

            // ✅ Aynı gün birden fazla çalışma aralığını destekle
            var availabilities = await _context.TrainerAvailabilities
                .Where(a => a.TrainerId == trainerId && a.DayOfWeek == selectedDate.DayOfWeek)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            if (availabilities.Count == 0)
                return Ok(new List<string>()); // O gün çalışmıyor

            // ✅ Sadece ONAYLI randevular slot kapatsın
            var approved = await _context.Appointments
                .Where(a => a.TrainerId == trainerId
                            && a.Status == AppointmentStatus.Approved
                            && a.StartDateTime.Date == selectedDate.Date)
                .Select(a => new { a.StartDateTime, a.EndDateTime })
                .ToListAsync();

            var result = new List<string>();

            foreach (var av in availabilities)
            {
                // av.StartTime - av.EndTime aralığında slot üret
                for (var t = av.StartTime; t.Add(step) <= av.EndTime; t = t.Add(step))
                {
                    var slotStart = selectedDate.Date + t;  // LOCAL
                    var slotEnd = slotStart.Add(step);

                    bool clash = approved.Any(a =>
                        slotStart < a.EndDateTime && slotEnd > a.StartDateTime);

                    if (!clash)
                        result.Add(slotStart.ToString("yyyy-MM-ddTHH:mm")); // "Z" yok (UTC değil)
                }
            }

            return Ok(result);
        }

        // 3) Alias (geri uyumluluk)
        // GET: /api/trainers/slots?trainerId=1&date=2025-12-27&duration=60
        [HttpGet("slots")]
        public Task<IActionResult> GetSlotsAlias(
            int trainerId, string date, int duration, int? serviceId = null)
            => GetFreeSlots(trainerId, date, duration, serviceId);
    }
}