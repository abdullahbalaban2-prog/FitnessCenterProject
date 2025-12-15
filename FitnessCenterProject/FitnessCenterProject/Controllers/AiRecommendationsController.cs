using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FitnessCenterProject.Data;
using FitnessCenterProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers
{
    [Authorize]
    public class AiRecommendationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AiRecommendationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AiRecommendations/Create
        public IActionResult Create()
        {
            var model = new AiRecommendation
            {
                Height = 170,
                Weight = 70,
                BodyType = "Normal",
                Goal = "Genel",
                RequestType = "Egzersiz + Diyet"
            };

            return View(model);
        }

        // POST: /AiRecommendations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AiRecommendation model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            // Kullanıcıdan gelmiş olabilecek validation hatasını temizle
            // (UserId formdan gelmeyecek, biz server'da set ediyoruz)
            ModelState.Remove(nameof(AiRecommendation.UserId));

            // Server tarafı alanlar
            model.UserId = userId;
            model.CreatedAt = DateTime.Now;

            // Demo AI çıktısı (boş gelirse üret)
            if (string.IsNullOrWhiteSpace(model.GeneratedPlan))
            {
                model.GeneratedPlan =
                    $"[Demo AI]\n" +
                    $"Boy: {model.Height} cm, Kilo: {model.Weight} kg\n" +
                    $"Vücut Tipi: {model.BodyType}\n" +
                    $"Hedef: {model.Goal}\n" +
                    $"İstek: {model.RequestType}\n\n" +
                    $"Öneri:\n" +
                    $"- 3 gün full-body (squat, bench, row)\n" +
                    $"- 2 gün 30 dk yürüyüş/kardiyo\n" +
                    $"- Günlük protein: 1.6g/kg hedefleyin.";
            }

            if (!ModelState.IsValid)
                return View(model);

            _context.AiRecommendations.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyRecommendations));
        }

        // GET: /AiRecommendations/MyRecommendations
        public async Task<IActionResult> MyRecommendations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var list = await _context.AiRecommendations
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(list);
        }
    }
}
