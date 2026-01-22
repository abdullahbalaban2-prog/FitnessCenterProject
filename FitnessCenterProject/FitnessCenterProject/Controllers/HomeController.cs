using FitnessCenterProject.Data;
using FitnessCenterProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FitnessCenterProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                FeaturedProjects = await _context.Projects
                    .Include(p => p.Category)
                    .Where(p => p.IsFeatured)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(6)
                    .ToListAsync(),
                Services = await _context.Services
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SortOrder)
                    .ToListAsync(),
                Testimonials = await _context.Testimonials
                    .Where(t => t.IsApproved)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(3)
                    .ToListAsync()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Models.ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
