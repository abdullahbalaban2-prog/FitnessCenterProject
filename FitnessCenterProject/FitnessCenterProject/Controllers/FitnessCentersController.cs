using System.Linq;
using System.Threading.Tasks;
using FitnessCenterProject.Data;
using FitnessCenterProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers
{
    // sadece admin erişsin
    [Authorize(Roles = "Admin")]
    public class FitnessCentersController : Controller
    {
        private readonly ApplicationDbContext _context;

        //dependency injection 
        public FitnessCentersController(ApplicationDbContext context)
        {
            _context = context;
        }

       
        // Tüm salonları listele
        public async Task<IActionResult> Index()
        {
            var centers = await _context.FitnessCenters.ToListAsync();
            return View(centers);
        }

        
        // Tek bir salonun detayları
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var center = await _context.FitnessCenters
                .FirstOrDefaultAsync(m => m.Id == id);

            if (center == null)
                return NotFound();

            return View(center);
        }

        
        // Yeni salon ekleme formu
        public IActionResult Create()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FitnessCenter fitnessCenter)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fitnessCenter);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata varsa formu aynı modelle geri göster
            return View(fitnessCenter);
        }

        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var center = await _context.FitnessCenters.FindAsync(id);
            if (center == null)
                return NotFound();

            return View(center);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FitnessCenter fitnessCenter)
        {
            if (id != fitnessCenter.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fitnessCenter);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FitnessCenterExists(fitnessCenter.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(fitnessCenter);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var center = await _context.FitnessCenters
                .FirstOrDefaultAsync(m => m.Id == id);

            if (center == null)
                return NotFound();

            return View(center);
        }

       
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var center = await _context.FitnessCenters.FindAsync(id);
            if (center != null)
            {
                _context.FitnessCenters.Remove(center);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FitnessCenterExists(int id)
        {
            return _context.FitnessCenters.Any(e => e.Id == id);
        }
    }
}
