using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FitnessCenterProject.Data;
using FitnessCenterProject.Models;
using FitnessCenterProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers
{
    [Authorize]
    public class AiRecommendationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiService _aiService;

        public AiRecommendationsController(ApplicationDbContext context, IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        public IActionResult Create()
        {
            return View(new AiRecommendation());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AiRecommendation model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            ModelState.Remove(nameof(AiRecommendation.UserId));

            model.UserId = userId;
            model.CreatedAt = DateTime.Now;

            var prompt =
                $"Boy: {model.Height} cm\n" +
                $"Kilo: {model.Weight} kg\n" +
                $"Vücut Tipi: {model.BodyType}\n" +
                $"Hedef: {model.Goal}\n" +
                $"İstek Tipi: {model.RequestType}\n" +
                $"Bu bilgilere göre kişisel fitness ve diyet planı öner.";

            model.GeneratedPlan = await _aiService.GeneratePlanAsync(prompt);


            if (!ModelState.IsValid)
                return View(model);

            _context.AiRecommendations.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyRecommendations));
        }

        public async Task<IActionResult> MyRecommendations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var list = await _context.AiRecommendations
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(list);
        }
    }
}
