using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FitnessCenterProject.Data;
using FitnessCenterProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // -------------------------------
        // ADMIN: TÜM RANDEVULAR
        // -------------------------------
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

        // -------------------------------
        // ÜYE: KENDİ RANDEVULARI
        // -------------------------------
        public async Task<IActionResult> MyAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var myAppointments = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .Where(a => a.MemberId == userId)
                .OrderByDescending(a => a.StartDateTime)
                .ToListAsync();

            return View(myAppointments);
        }

        // -------------------------------
        // CREATE (GET)
        // -------------------------------
        public async Task<IActionResult> Create()
        {
            await LoadServicesAsync(); // ViewBag.Services = List<Service>
            ViewBag.Trainers = new SelectList(Enumerable.Empty<SelectListItem>());

            var model = new Appointment
            {
                StartDateTime = DateTime.Now.AddHours(1)
            };

            return View(model);
        }

        // -------------------------------
        // CREATE (POST)
        // -------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            // Sayfa geri dönerse hizmetler yine dolu kalsın
            await LoadServicesAsync();

            // Trainer dropdown: seçilen ServiceId’ye göre tekrar dolsun (formda eğitmen seçimi kaybolmasın)
            if (appointment.ServiceId > 0)
                await LoadTrainersByServiceAsync(appointment.ServiceId, appointment.TrainerId);
            else
                ViewBag.Trainers = new SelectList(Enumerable.Empty<SelectListItem>());

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            appointment.MemberId = userId;

            // Servisten fiyat + süre çek
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == appointment.ServiceId);
            if (service == null)
            {
                ModelState.AddModelError("ServiceId", "Geçerli bir hizmet seçmelisiniz.");
            }
            else
            {
                appointment.Price = service.Price;
                appointment.EndDateTime = appointment.StartDateTime.AddMinutes(service.DurationMinutes);
            }

            appointment.Status = AppointmentStatus.Pending;

            if (!ModelState.IsValid)
                return View(appointment);

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyAppointments));
        }

        // -------------------------------
        // ADMIN: DETAILS
        // -------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // -------------------------------
        // ADMIN: EDIT (GET)
        // -------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            await LoadServicesAsync();

            // Seçili hizmete göre eğitmenleri doldur (yoksa boş)
            if (appointment.ServiceId > 0)
                await LoadTrainersByServiceAsync(appointment.ServiceId, appointment.TrainerId);
            else
                ViewBag.Trainers = new SelectList(Enumerable.Empty<SelectListItem>());

            return View(appointment);
        }

        // -------------------------------
        // ADMIN: EDIT (POST)
        // -------------------------------
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            if (id != appointment.Id) return NotFound();

            await LoadServicesAsync();
            if (appointment.ServiceId > 0)
                await LoadTrainersByServiceAsync(appointment.ServiceId, appointment.TrainerId);
            else
                ViewBag.Trainers = new SelectList(Enumerable.Empty<SelectListItem>());

            if (!ModelState.IsValid)
                return View(appointment);

            try
            {
                _context.Update(appointment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(appointment.Id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // -------------------------------
        // ADMIN: DELETE (GET)
        // -------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // -------------------------------
        // ADMIN: DELETE (POST)
        // -------------------------------
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

        // -------------------------------
        // ADMIN: APPROVE / REJECT
        // -------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = AppointmentStatus.Approved;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = AppointmentStatus.Rejected;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // HELPERS
        // =========================

        // Create.cshtml senin yazdığın gibi services listesini kullanıyor:
        // var services = ViewBag.Services as IEnumerable<Service>
        private async Task LoadServicesAsync()
        {
            ViewBag.Services = await _context.Services
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        // Eğitmenleri seçilen hizmete göre doldur
        private async Task LoadTrainersByServiceAsync(int serviceId, int? selectedTrainerId = null)
        {
            var trainers = await _context.TrainerServices
                .Where(ts => ts.ServiceId == serviceId)
                .Select(ts => ts.Trainer!)
                .Distinct()
                .OrderBy(t => t.FirstName)
                .ThenBy(t => t.LastName)
                .Select(t => new
                {
                    t.Id,
                    FullName = t.FirstName + " " + t.LastName
                })
                .ToListAsync();

            ViewBag.Trainers = new SelectList(trainers, "Id", "FullName", selectedTrainerId);
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
