using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FitnessCenterProject.Data;
using FitnessCenterProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------------
        // Üyenin kendi randevuları
        // ------------------------------------------------------------
        public async Task<IActionResult> MyAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var list = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .Where(a => a.MemberId == userId)
                .OrderByDescending(a => a.StartDateTime)
                .ToListAsync();

            return View(list);
        }

        // ------------------------------------------------------------
        // Randevu oluşturma (form)
        // ------------------------------------------------------------
        public async Task<IActionResult> Create()
        {
            ViewBag.Services = await _context.Services.ToListAsync();
            return View(new Appointment());
        }

        // ------------------------------------------------------------
        // (Opsiyonel) Form submit ile genel kayıt
        // Not: JS ile "slot seçimi" zaten Start/End'i dolduruyor.
        // ------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment model)
        {
            // POST'ta ViewBag boş kalıp NullReference vermesin
            ViewBag.Services = await _context.Services.ToListAsync();

            var service = await _context.Services.FindAsync(model.ServiceId);
            if (service == null)
            {
                ModelState.AddModelError("", "Hizmet bulunamadı.");
                return View(model);
            }

            // ✅ 3 saat geri kayma fix:
            // Eğer istemciden UTC (Z) gelmişse yerel saate çevir
            if (model.StartDateTime.Kind == DateTimeKind.Utc)
                model.StartDateTime = model.StartDateTime.ToLocalTime();

            // Süre + bitiş
            var end = model.StartDateTime.AddMinutes(service.DurationMinutes);

            // ✅ Eğitmen o gün çalışıyor mu? (Müsaitlik kontrolü)
            var availability = await _context.TrainerAvailabilities.FirstOrDefaultAsync(a =>
                a.TrainerId == model.TrainerId &&
                a.DayOfWeek == model.StartDateTime.DayOfWeek);

            if (availability == null)
            {
                ModelState.AddModelError("", "Eğitmen bu gün çalışmıyor.");
                return View(model);
            }

            // Seçilen saat çalışma aralığında mı?
            if (model.StartDateTime.TimeOfDay < availability.StartTime || end.TimeOfDay > availability.EndTime)
            {
                ModelState.AddModelError("", "Seçtiğiniz saat eğitmenin çalışma saatleri dışında.");
                return View(model);
            }

            // ✅ Slot çakışması: sadece ONAYLI randevular saati kapatsın
            var conflict = await _context.Appointments.AnyAsync(a =>
                a.TrainerId == model.TrainerId &&
                a.Status == AppointmentStatus.Approved &&
                model.StartDateTime < a.EndDateTime &&
                end > a.StartDateTime);

            if (conflict)
            {
                ModelState.AddModelError("", "Bu saat dolu. Lütfen başka bir saat seçin.");
                return View(model);
            }

            model.MemberId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.Price = service.Price;
            model.EndDateTime = end;
            model.Status = AppointmentStatus.Pending;

            _context.Appointments.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyAppointments));
        }

        // ------------------------------------------------------------
        // Slot sayfasından direkt kayıt (hidden alanlarla)
        // ------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromSlot(int serviceId, int trainerId, string start)
        {
            // start = "yyyy-MM-ddTHH:mm" (Z YOK, YEREL SAAT)
            if (!DateTime.TryParse(start, out var startLocal))
            {
                TempData["Error"] = "Geçersiz tarih formatı.";
                return RedirectToAction(nameof(Create));
            }

            var service = await _context.Services.FindAsync(serviceId);
            if (service == null)
            {
                TempData["Error"] = "Hizmet bulunamadı.";
                return RedirectToAction(nameof(Create));
            }

            var endLocal = startLocal.AddMinutes(service.DurationMinutes);

            var appt = new Appointment
            {
                MemberId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                TrainerId = trainerId,
                ServiceId = serviceId,
                StartDateTime = startLocal,   // yerel saat
                EndDateTime = endLocal,
                Price = service.Price,
                Status = AppointmentStatus.Pending
            };

            _context.Appointments.Add(appt);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyAppointments));
        }

        // ------------------------------------------------------------
        // ADMIN — tüm randevular
        // ------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var list = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .OrderByDescending(a => a.StartDateTime)
                .ToListAsync();

            return View(list);
        }

        // ------------------------------------------------------------
        // ADMIN — Onayla (bloklama kuralı burada devreye girer)
        // ------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var appt = await _context.Appointments
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appt == null) return NotFound();

            // Aynı eğitmen, aynı zaman diliminde onaylı randevu var mı?
            bool hasApprovedClash = await _context.Appointments.AnyAsync(a =>
                a.Id != appt.Id &&
                a.TrainerId == appt.TrainerId &&
                a.Status == AppointmentStatus.Approved &&
                a.StartDateTime < appt.EndDateTime &&
                a.EndDateTime > appt.StartDateTime);

            if (hasApprovedClash)
            {
                TempData["Error"] = "Bu saat aralığı başka bir onaylı randevu ile çakışıyor.";
                return RedirectToAction(nameof(Index));
            }

            appt.Status = AppointmentStatus.Approved;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ------------------------------------------------------------
        // ADMIN — Reddet
        // ------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            appt.Status = AppointmentStatus.Rejected;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ------------------------------------------------------------
        // ADMIN — Detay
        // ------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var appt = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appt == null) return NotFound();
            return View(appt);
        }

        // ------------------------------------------------------------
        // ADMIN — Düzenle
        // ------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            ViewBag.MemberId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.Users, "Id", "Email", appt.MemberId);

            ViewBag.TrainerId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.Trainers, "Id", "FullName", appt.TrainerId);

            ViewBag.ServiceId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.Services, "Id", "Name", appt.ServiceId);

            return View(appt);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Appointment model)
        {
            var service = await _context.Services.FindAsync(model.ServiceId);
            if (service == null) return NotFound();

            model.EndDateTime = model.StartDateTime.AddMinutes(service.DurationMinutes);
            model.Price = service.Price;

            // Onaylısa ve saatler değiştiyse çakışma kontrolü yine yapılmalı
            if (model.Status == AppointmentStatus.Approved)
            {
                bool hasApprovedClash = await _context.Appointments.AnyAsync(a =>
                    a.Id != model.Id &&
                    a.TrainerId == model.TrainerId &&
                    a.Status == AppointmentStatus.Approved &&
                    a.StartDateTime < model.EndDateTime &&
                    a.EndDateTime > model.StartDateTime);

                if (hasApprovedClash)
                {
                    TempData["Error"] = "Bu saat aralığı başka bir onaylı randevu ile çakışıyor.";
                    return RedirectToAction(nameof(Index));
                }
            }

            _context.Appointments.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ------------------------------------------------------------
        // ADMIN — Sil
        // ------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var appt = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appt == null) return NotFound();
            return View(appt);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt != null)
            {
                _context.Appointments.Remove(appt);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
