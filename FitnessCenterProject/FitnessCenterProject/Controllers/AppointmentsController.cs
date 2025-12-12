using FitnessCenterProject.Data;
using FitnessCenterProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers
{
    // Şimdilik randevu yönetimi Admin'e özel olsun
    [Authorize(Roles = "Admin")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            // Randevuları Member, Trainer ve Service ile birlikte çekiyoruz
            var appointments = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .ToListAsync();

            return View(appointments);
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // Yardımcı metod: Dropdown listeleri doldur
        private void PopulateDropDowns(Appointment? appointment = null)
        {
            // Üyeler (ApplicationUser)
            var users = _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    DisplayName = string.IsNullOrEmpty(u.Name) ? u.Email : u.Name
                })
                .ToList();

            ViewData["MemberId"] = new SelectList(users, "Id", "DisplayName", appointment?.MemberId);

            // Eğitmenler
            var trainers = _context.Trainers
                .Select(t => new
                {
                    t.Id,
                    FullName = t.FirstName + " " + t.LastName
                })
                .ToList();

            ViewData["TrainerId"] = new SelectList(trainers, "Id", "FullName", appointment?.TrainerId);

            // Hizmetler
            var services = _context.Services
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .ToList();

            ViewData["ServiceId"] = new SelectList(services, "Id", "Name", appointment?.ServiceId);
        }

        // GET: Appointments/Create
        public IActionResult Create()
        {
            PopulateDropDowns();
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            // Service bilgisine göre Price ve EndDateTime hesaplayalım
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == appointment.ServiceId);

            if (service != null)
            {
                appointment.Price = service.Price;
                appointment.EndDateTime = appointment.StartDateTime.AddMinutes(service.DurationMinutes);
            }

            // Yeni randevu varsayılan olarak Pending olsun
            appointment.Status = AppointmentStatus.Pending;

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Validasyon hatası varsa dropdownları tekrar doldur
            PopulateDropDowns(appointment);
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            PopulateDropDowns(appointment);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            if (id != appointment.Id)
                return NotFound();

            // Service değiştiyse tekrar fiyat ve bitiş zamanı hesaplayalım
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == appointment.ServiceId);

            if (service != null)
            {
                appointment.Price = service.Price;
                appointment.EndDateTime = appointment.StartDateTime.AddMinutes(service.DurationMinutes);
            }

            if (ModelState.IsValid)
            {
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

            PopulateDropDowns(appointment);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // POST: Appointments/Delete/5 (Confirm)
        [HttpPost, ActionName("Delete")]
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

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
