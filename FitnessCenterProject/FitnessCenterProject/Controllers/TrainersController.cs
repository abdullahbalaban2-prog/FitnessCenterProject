using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessCenterProject.Data;
using FitnessCenterProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TrainersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var trainers = await _context.Trainers
                .Include(t => t.FitnessCenter)
                .ToListAsync();

            return View(trainers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _context.Trainers
                .Include(t => t.FitnessCenter)
                .Include(t => t.TrainerServices!)
                    .ThenInclude(ts => ts.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainer == null) return NotFound();

            return View(trainer);
        }

        public async Task<IActionResult> Create()
        {
            PopulateFitnessCentersDropDownList();
            await PopulateServicesAsync(selectedIds: null);
            return View(new Trainer());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,FirstName,LastName,Bio,Specialty,FitnessCenterId")] Trainer trainer,
            int[] selectedServices,
            bool createDefaultAvailability = true)
        {
            PopulateFitnessCentersDropDownList(trainer.FitnessCenterId);
            await PopulateServicesAsync(selectedServices);

            if (!ModelState.IsValid)
                return View(trainer);

            _context.Trainers.Add(trainer);
            await _context.SaveChangesAsync();

            await SetTrainerServicesAsync(trainer.Id, selectedServices);

            if (createDefaultAvailability)
                await EnsureDefaultAvailabilitiesAsync(trainer.Id);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _context.Trainers
                .Include(t => t.TrainerServices)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null) return NotFound();

            var selected = trainer.TrainerServices?.Select(ts => ts.ServiceId).ToArray() ?? Array.Empty<int>();

            PopulateFitnessCentersDropDownList(trainer.FitnessCenterId);
            await PopulateServicesAsync(selected);

            return View(trainer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,FirstName,LastName,Bio,Specialty,FitnessCenterId")] Trainer trainer,
            int[] selectedServices,
            bool createDefaultAvailability = false)
        {
            if (id != trainer.Id) return NotFound();

            PopulateFitnessCentersDropDownList(trainer.FitnessCenterId);
            await PopulateServicesAsync(selectedServices);

            if (!ModelState.IsValid)
                return View(trainer);

            _context.Update(trainer);
            await _context.SaveChangesAsync();

            await SetTrainerServicesAsync(trainer.Id, selectedServices);

            if (createDefaultAvailability)
                await EnsureDefaultAvailabilitiesAsync(trainer.Id);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _context.Trainers
                .Include(t => t.FitnessCenter)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainer == null) return NotFound();

            return View(trainer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Eğitmene bağlı randevu varsa silme (FK kırılmasın)
            var hasAppointments = await _context.Appointments.AnyAsync(a => a.TrainerId == id);
            if (hasAppointments)
            {
                TempData["Error"] = "Bu eğitmene bağlı randevular olduğu için silinemez.";
                return RedirectToAction(nameof(Index));
            }

            // Join kayıtlarını sil (NoAction olduğu için şart)
            var ts = await _context.TrainerServices.Where(x => x.TrainerId == id).ToListAsync();
            _context.TrainerServices.RemoveRange(ts);

            var av = await _context.TrainerAvailabilities.Where(x => x.TrainerId == id).ToListAsync();
            _context.TrainerAvailabilities.RemoveRange(av);

            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer != null)
            {
                _context.Trainers.Remove(trainer);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private void PopulateFitnessCentersDropDownList(object? selectedCenter = null)
        {
            var centers = _context.FitnessCenters
                .OrderBy(c => c.Name)
                .ToList();

            ViewData["FitnessCenterId"] = new SelectList(centers, "Id", "Name", selectedCenter);
        }

        private async Task PopulateServicesAsync(int[]? selectedIds)
        {
            var services = await _context.Services
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.ServicesList = services;
            ViewBag.SelectedServices = (selectedIds ?? Array.Empty<int>()).ToHashSet();
        }

        private async Task SetTrainerServicesAsync(int trainerId, int[] serviceIds)
        {
            var existing = await _context.TrainerServices
                .Where(x => x.TrainerId == trainerId)
                .ToListAsync();

            _context.TrainerServices.RemoveRange(existing);

            var ids = (serviceIds ?? Array.Empty<int>())
                .Distinct()
                .ToList();

            if (ids.Count == 0) return;

            // Sadece gerçekten var olan servisleri bağla
            var validIds = await _context.Services
                .Where(s => ids.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();

            foreach (var sid in validIds)
                _context.TrainerServices.Add(new TrainerService { TrainerId = trainerId, ServiceId = sid });
        }

        private async Task EnsureDefaultAvailabilitiesAsync(int trainerId)
        {
            var start = new TimeSpan(8, 0, 0);
            var end = new TimeSpan(22, 0, 0);

            var days = new[]
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday
            };

            foreach (var d in days)
            {
                var exists = await _context.TrainerAvailabilities.AnyAsync(a =>
                    a.TrainerId == trainerId && a.DayOfWeek == d);

                if (!exists)
                {
                    _context.TrainerAvailabilities.Add(new TrainerAvailability
                    {
                        TrainerId = trainerId,
                        DayOfWeek = d,
                        StartTime = start,
                        EndTime = end
                    });
                }
            }
        }
    }
}