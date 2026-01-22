using FitnessCenterProject.Data;
using FitnessCenterProject.Models;
using FitnessCenterProject.Utilities;
using FitnessCenterProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("admin/categories")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var categories = await _context.PortfolioCategories
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View("~/Views/Admin/Categories/Index.cshtml", categories);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Categories/Create.cshtml", new CategoryViewModel());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Categories/Create.cshtml", model);
            }

            var slug = await GenerateUniqueSlugAsync(model.Name);

            var category = new PortfolioCategory
            {
                Name = model.Name,
                Slug = slug,
                CreatedAt = DateTime.UtcNow
            };

            _context.PortfolioCategories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kategori oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.PortfolioCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var model = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name
            };

            return View("~/Views/Admin/Categories/Edit.cshtml", model);
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Categories/Edit.cshtml", model);
            }

            var category = await _context.PortfolioCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.Name = model.Name;
            category.Slug = await GenerateUniqueSlugAsync(model.Name, category.Id);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Kategori güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.PortfolioCategories
                .Include(c => c.Projects)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            if (category.Projects != null && category.Projects.Any())
            {
                TempData["Error"] = "Bu kategoriye bağlı projeler olduğu için silinemez.";
                return RedirectToAction(nameof(Index));
            }

            _context.PortfolioCategories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kategori silindi.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GenerateUniqueSlugAsync(string name, int? currentId = null)
        {
            var baseSlug = SlugHelper.GenerateSlug(name);
            if (string.IsNullOrWhiteSpace(baseSlug))
            {
                baseSlug = Guid.NewGuid().ToString("N");
            }

            var slug = baseSlug;
            var counter = 1;

            while (await _context.PortfolioCategories.AnyAsync(c => c.Slug == slug && c.Id != currentId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }
    }
}
