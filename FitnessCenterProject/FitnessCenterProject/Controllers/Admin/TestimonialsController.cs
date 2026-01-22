using FitnessCenterProject.Data;
using FitnessCenterProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("admin/testimonials")]
    public class TestimonialsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestimonialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var testimonials = await _context.Testimonials
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View("~/Views/Admin/Testimonials/Index.cshtml", testimonials);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Testimonials/Create.cshtml", new TestimonialViewModel());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestimonialViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Testimonials/Create.cshtml", model);
            }

            var testimonial = new Models.Testimonial
            {
                FullName = model.FullName,
                CompanyOrProject = model.CompanyOrProject,
                Comment = model.Comment,
                Rating = model.Rating,
                IsApproved = model.IsApproved,
                CreatedAt = DateTime.UtcNow
            };

            _context.Testimonials.Add(testimonial);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Referans eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("approve/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial == null)
            {
                return NotFound();
            }

            testimonial.IsApproved = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Referans onaylandı.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("reject/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial == null)
            {
                return NotFound();
            }

            testimonial.IsApproved = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Referans yayından kaldırıldı.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial == null)
            {
                return NotFound();
            }

            _context.Testimonials.Remove(testimonial);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Referans silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
