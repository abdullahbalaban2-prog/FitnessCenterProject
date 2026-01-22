using FitnessCenterProject.Data;
using FitnessCenterProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                ProjectCount = await _context.Projects.CountAsync(),
                UnreadMessages = await _context.ContactMessages.CountAsync(m => !m.IsRead),
                UnreadQuotes = await _context.QuoteRequests.CountAsync(q => !q.IsRead),
                PendingTestimonials = await _context.Testimonials.CountAsync(t => !t.IsApproved)
            };

            return View("~/Views/Admin/Dashboard/Index.cshtml", model);
        }
    }
}
