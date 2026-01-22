using FitnessCenterProject.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("admin/quotes")]
    public class QuoteRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuoteRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var quotes = await _context.QuoteRequests
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return View("~/Views/Admin/QuoteRequests/Index.cshtml", quotes);
        }

        [HttpGet("details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var quote = await _context.QuoteRequests.FindAsync(id);
            if (quote == null)
            {
                return NotFound();
            }

            if (!quote.IsRead)
            {
                quote.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return View("~/Views/Admin/QuoteRequests/Details.cshtml", quote);
        }

        [HttpPost("mark-read/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            var quote = await _context.QuoteRequests.FindAsync(id);
            if (quote == null)
            {
                return NotFound();
            }

            quote.IsRead = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Teklif talebi okundu olarak işaretlendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var quote = await _context.QuoteRequests.FindAsync(id);
            if (quote == null)
            {
                return NotFound();
            }

            _context.QuoteRequests.Remove(quote);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Teklif talebi silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
