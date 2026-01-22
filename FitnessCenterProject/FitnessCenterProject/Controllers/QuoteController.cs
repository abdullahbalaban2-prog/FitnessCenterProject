using FitnessCenterProject.Data;
using FitnessCenterProject.Models;
using FitnessCenterProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterProject.Controllers
{
    public class QuoteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuoteController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(new QuoteRequestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(QuoteRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = new QuoteRequest
            {
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                ProjectType = model.ProjectType,
                BudgetRange = model.BudgetRange,
                DesiredDate = model.DesiredDate,
                Message = model.Message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.QuoteRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Teklif talebiniz alındı. En kısa sürede dönüş yapacağız.";
            return RedirectToAction(nameof(Index));
        }
    }
}
