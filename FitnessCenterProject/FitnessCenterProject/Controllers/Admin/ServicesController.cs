using FitnessCenterProject.Data;
using FitnessCenterProject.Models;
using FitnessCenterProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("admin/services")]
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            return View("~/Views/Admin/Services/Index.cshtml", services);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Services/Create.cshtml", new ServiceViewModel { IsActive = true });
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Services/Create.cshtml", model);
            }

            var service = new Service
            {
                Title = model.Title,
                Description = model.Description,
                IconName = model.IconName,
                SortOrder = model.SortOrder,
                IsActive = model.IsActive
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hizmet oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            var model = new ServiceViewModel
            {
                Id = service.Id,
                Title = service.Title,
                Description = service.Description,
                IconName = service.IconName,
                SortOrder = service.SortOrder,
                IsActive = service.IsActive
            };

            return View("~/Views/Admin/Services/Edit.cshtml", model);
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Services/Edit.cshtml", model);
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            service.Title = model.Title;
            service.Description = model.Description;
            service.IconName = model.IconName;
            service.SortOrder = model.SortOrder;
            service.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Hizmet güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hizmet silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
