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
    public class TrainerAvailabilitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainerAvailabilitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /TrainerAvailabilities?trainerId=3
        public async Task<IActionResult> Index(int? trainerId)
        {
            await LoadTrainers(trainerId);
            ViewBag.SelectedTrainerId = trainerId;

            var query = _context.TrainerAvailabilities
                .Include(t => t.Trainer)
                .AsQueryable();

            if (trainerId.HasValue)
                query = query.Where(x => x.TrainerId == trainerId.Value);

            var list = await query
                .OrderBy(a => a.Trainer!.FirstName)
                .ThenBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            return View(list);
        }

        // GET: /TrainerAvailabilities/Create?trainerId=3
        public async Task<IActionResult> Create(int? trainerId)
        {
            await LoadTrainers(trainerId);
            ViewBag.SelectedTrainerId = trainerId;
            return View(new TrainerAvailability { TrainerId = trainerId ?? 0 });
        }

        // POST: /TrainerAvailabilities/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainerAvailability model)
        {
            await LoadTrainers(model.TrainerId);
            ViewBag.SelectedTrainerId = model.TrainerId;

            if (model.EndTime <= model.StartTime)
                ModelState.AddModelError(nameof(TrainerAvailability.EndTime), "Bitiş saati başlangıçtan büyük olmalıdır.");

            // Aynı eğitmen + aynı gün için çakışan saat aralığı var mı?
            var overlap = await _context.TrainerAvailabilities.AnyAsync(a =>
                a.TrainerId == model.TrainerId &&
                a.DayOfWeek == model.DayOfWeek &&
                model.StartTime < a.EndTime &&
                model.EndTime > a.StartTime);

            if (overlap)
                ModelState.AddModelError("", "Bu gün için seçtiğiniz saat aralığı mevcut bir müsaitlik ile çakışıyor.");

            if (!ModelState.IsValid)
                return View(model);

            _context.TrainerAvailabilities.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { trainerId = model.TrainerId });
        }

        // GET: /TrainerAvailabilities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.TrainerAvailabilities.FindAsync(id);
            if (model == null) return NotFound();

            await LoadTrainers(model.TrainerId);
            ViewBag.SelectedTrainerId = model.TrainerId;

            return View(model);
        }

        // POST: /TrainerAvailabilities/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrainerAvailability model)
        {
            if (id != model.Id) return NotFound();

            await LoadTrainers(model.TrainerId);
            ViewBag.SelectedTrainerId = model.TrainerId;

            if (model.EndTime <= model.StartTime)
                ModelState.AddModelError(nameof(TrainerAvailability.EndTime), "Bitiş saati başlangıçtan büyük olmalıdır.");

            var overlap = await _context.TrainerAvailabilities.AnyAsync(a =>
                a.Id != model.Id &&
                a.TrainerId == model.TrainerId &&
                a.DayOfWeek == model.DayOfWeek &&
                model.StartTime < a.EndTime &&
                model.EndTime > a.StartTime);

            if (overlap)
                ModelState.AddModelError("", "Bu gün için seçtiğiniz saat aralığı mevcut bir müsaitlik ile çakışıyor.");

            if (!ModelState.IsValid)
                return View(model);

            _context.Update(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { trainerId = model.TrainerId });
        }

        // GET: /TrainerAvailabilities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.TrainerAvailabilities
                .Include(x => x.Trainer)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (model == null) return NotFound();

            ViewBag.SelectedTrainerId = model.TrainerId;
            return View(model);
        }

        // POST: /TrainerAvailabilities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.TrainerAvailabilities.FindAsync(id);
            if (model != null)
            {
                var trainerId = model.TrainerId;
                _context.TrainerAvailabilities.Remove(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { trainerId });
            }

            return RedirectToAction(nameof(Index));
        }

        // ✅ Tek tıkla varsayılan program: 08:00-22:00 Pzt-Cmt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDefault(int trainerId)
        {
            var trainerExists = await _context.Trainers.AnyAsync(t => t.Id == trainerId);
            if (!trainerExists) return NotFound();

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
                    a.TrainerId == trainerId &&
                    a.DayOfWeek == d);

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

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { trainerId });
        }

        private async Task LoadTrainers(int? selectedId = null)
        {
            var trainers = await _context.Trainers
                .OrderBy(t => t.FirstName)
                .ThenBy(t => t.LastName)
                .Select(t => new { t.Id, Name = t.FirstName + " " + t.LastName })
                .ToListAsync();

            ViewBag.TrainerId = new SelectList(trainers, "Id", "Name", selectedId);
        }
    }
}