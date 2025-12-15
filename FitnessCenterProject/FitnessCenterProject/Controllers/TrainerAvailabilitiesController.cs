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

        // GET: /TrainerAvailabilities
        public async Task<IActionResult> Index()
        {
            var list = await _context.TrainerAvailabilities
                .Include(t => t.Trainer)
                .OrderBy(a => a.Trainer!.FirstName)
                .ThenBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            return View(list);
        }

        // GET: /TrainerAvailabilities/Create
        public async Task<IActionResult> Create()
        {
            await LoadTrainers();
            return View(new TrainerAvailability());
        }

        // POST: /TrainerAvailabilities/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainerAvailability model)
        {
            await LoadTrainers(model.TrainerId);

            if (model.EndTime <= model.StartTime)
                ModelState.AddModelError(nameof(TrainerAvailability.EndTime), "Bitiş saati başlangıçtan büyük olmalıdır.");

            if (!ModelState.IsValid)
                return View(model);

            _context.TrainerAvailabilities.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /TrainerAvailabilities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.TrainerAvailabilities.FindAsync(id);
            if (model == null) return NotFound();

            await LoadTrainers(model.TrainerId);
            return View(model);
        }

        // POST: /TrainerAvailabilities/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrainerAvailability model)
        {
            if (id != model.Id) return NotFound();

            await LoadTrainers(model.TrainerId);

            if (model.EndTime <= model.StartTime)
                ModelState.AddModelError(nameof(TrainerAvailability.EndTime), "Bitiş saati başlangıçtan büyük olmalıdır.");

            if (!ModelState.IsValid)
                return View(model);

            _context.Update(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /TrainerAvailabilities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.TrainerAvailabilities
                .Include(x => x.Trainer)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (model == null) return NotFound();

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
                _context.TrainerAvailabilities.Remove(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
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
