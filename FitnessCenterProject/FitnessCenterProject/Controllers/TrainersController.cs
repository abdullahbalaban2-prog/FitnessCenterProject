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

        // DbContext dependency injection
        public TrainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index()
        {
            // Eğitmenleri bağlı oldukları spor salonuyla birlikte çekiyoruz
            var trainers = await _context.Trainers
                .Include(t => t.FitnessCenter)
                .ToListAsync();

            return View(trainers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var trainer = await _context.Trainers
                .Include(t => t.FitnessCenter)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainer == null)
                return NotFound();

            return View(trainer);
        }

        
        public IActionResult Create()
        {
            PopulateFitnessCentersDropDownList();
            return View();
        }

        // Spor salonu dropdown'u için yardımcı metod
        private void PopulateFitnessCentersDropDownList(object? selectedCenter = null)
        {
            var centers = _context.FitnessCenters
                .OrderBy(c => c.Name)
                .ToList();

            ViewData["FitnessCenterId"] = new SelectList(centers, "Id", "Name", selectedCenter);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,FirstName,LastName,Bio,Specialty,FitnessCenterId")]
            Trainer trainer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trainer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Model hatalıysa, dropdown yeniden doldurulmalı
            PopulateFitnessCentersDropDownList(trainer.FitnessCenterId);
            return View(trainer);
        }

        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null)
                return NotFound();

            PopulateFitnessCentersDropDownList(trainer.FitnessCenterId);
            return View(trainer);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,FirstName,LastName,Bio,Specialty,FitnessCenterId")]
            Trainer trainer)
        {
            if (id != trainer.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainerExists(trainer.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            PopulateFitnessCentersDropDownList(trainer.FitnessCenterId);
            return View(trainer);
        }

        
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var trainer = await _context.Trainers
                .Include(t => t.FitnessCenter)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainer == null)
                return NotFound();

            return View(trainer);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer != null)
            {
                _context.Trainers.Remove(trainer);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TrainerExists(int id)
        {
            return _context.Trainers.Any(e => e.Id == id);
        }
    }
}
