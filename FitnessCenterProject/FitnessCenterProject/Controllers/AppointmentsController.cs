using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using FitnessCenterProject.Data;
using FitnessCenterProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers
{
    // Bu controller'daki TÜM aksiyonlar için giriş zorunlu
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------
        //  ADMIN: TÜM RANDEVULARI GÖR (Appointments/Index)
        // ------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .OrderByDescending(a => a.StartDateTime)
                .ToListAsync();

            return View(appointments);
        }

        // ------------------------------------------------------
        //  ÜYE: SADECE KENDİ RANDEVULARINI GÖRSÜN
        //  /Appointments/MyAppointments
        // ------------------------------------------------------
        public async Task<IActionResult> MyAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
                return Challenge();
            }

            var myAppointments = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .Where(a => a.MemberId == userId)
                .OrderByDescending(a => a.StartDateTime)
                .ToListAsync();

            return View(myAppointments);
        }

        // ------------------------------------------------------
        //  RANDEVU ALMA SAYFASI (GET)
        //  /Appointments/Create
        // ------------------------------------------------------
        public async Task<IActionResult> Create()
        {
            // Create ekranında eğitmen listesi API'den geleceği için
            // burada sadece hizmet listesini dolduruyoruz.
            await LoadDropdownsAsync(includeTrainers: false);

            var model = new Appointment
            {
                // Öntanımlı: 1 saat sonrası
                StartDateTime = DateTime.Now.AddHours(1)
            };

            return View(model);
        }

        // ------------------------------------------------------
        //  RANDEVU OLUŞTURMA (POST)
        // ------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            // Hata durumunda form tekrar çizilecek → Hizmetler yeniden gelsin
            await LoadDropdownsAsync(includeTrainers: false);

            // 1) Giriş yapan kullanıcının Id'si
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge(); // login sayfasına yönlendir
            }
            appointment.MemberId = userId;

            // 2) Seçilen hizmeti bul → fiyat ve süreyi buradan al
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == appointment.ServiceId);

            if (service == null)
            {
                ModelState.AddModelError("ServiceId", "Geçerli bir hizmet seçmelisiniz.");
            }
            else
            {
                // Ücret
                appointment.Price = service.Price;

                // Bitiş zamanı = başlangıç + hizmet süresi
                if (appointment.StartDateTime == default)
                {
                    ModelState.AddModelError("StartDateTime", "Başlangıç zamanı zorunludur.");
                }
                else
                {
                    appointment.EndDateTime = appointment
                        .StartDateTime
                        .AddMinutes(service.DurationMinutes);
                }
            }

            // 3) Başlangıçta durum Pending olsun
            appointment.Status = AppointmentStatus.Pending;

            // 4) Eğitmen için çakışan randevu var mı? (aynı zaman aralığında)
            if (ModelState.IsValid)
            {
                bool hasConflict = await _context.Appointments
                    .AnyAsync(a =>
                        a.TrainerId == appointment.TrainerId &&
                        a.Id != appointment.Id &&
                        // zaman çakışması kontrolü
                        a.StartDateTime < appointment.EndDateTime &&
                        appointment.StartDateTime < a.EndDateTime);

                if (hasConflict)
                {
                    ModelState.AddModelError(string.Empty,
                        "Seçilen eğitmen bu zaman aralığında başka bir randevuya sahip.");
                }
            }

            if (!ModelState.IsValid)
            {
                // Hatalar varsa formu aynı şekilde geri göster
                return View(appointment);
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Kullanıcı kendi randevu listesini görsün
            return RedirectToAction(nameof(MyAppointments));
        }

        // ------------------------------------------------------
        //  ADMIN: RANDEVU DETAY
        // ------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // ------------------------------------------------------
        //  ADMIN: RANDEVU DÜZENLEME (GET)
        // ------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            // Admin için hem hizmet hem eğitmen dropdown'ları dolu gelsin
            await LoadDropdownsAsync(includeTrainers: true);

            return View(appointment);
        }

        // ------------------------------------------------------
        //  ADMIN: RANDEVU DÜZENLEME (POST)
        // ------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            if (id != appointment.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(includeTrainers: true);
                return View(appointment);
            }

            try
            {
                _context.Update(appointment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(appointment.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ------------------------------------------------------
        //  ADMIN: RANDEVU SİLME (GET)
        // ------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // ------------------------------------------------------
        //  ADMIN: RANDEVU SİLME (POST)
        // ------------------------------------------------------
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ------------------------------------------------------
        //  ADMIN: RANDEVU ONAYLA
        // ------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = AppointmentStatus.Approved;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ------------------------------------------------------
        //  ADMIN: RANDEVU REDDET
        // ------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = AppointmentStatus.Rejected;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ------------------------------------------------------
        //  Yardımcı metotlar
        // ------------------------------------------------------

        // Hizmet & (gerekirse) Eğitmen dropdown'larını doldur
        private async Task LoadDropdownsAsync(bool includeTrainers)
        {
            ViewBag.Services = await _context.Services
                .OrderBy(s => s.Name)
                .ToListAsync();

            if (includeTrainers)
            {
                ViewBag.Trainers = await _context.Trainers
                    .OrderBy(t => t.FirstName)
                    .ThenBy(t => t.LastName)
                    .ToListAsync();
            }
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
