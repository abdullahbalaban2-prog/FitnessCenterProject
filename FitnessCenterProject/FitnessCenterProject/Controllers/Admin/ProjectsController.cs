using FitnessCenterProject.Data;
using FitnessCenterProject.Models;
using FitnessCenterProject.Utilities;
using FitnessCenterProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("admin/projects")]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProjectsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View("~/Views/Admin/Projects/Index.cshtml", projects);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAsync();
            return View("~/Views/Admin/Projects/Create.cshtml", new ProjectUpsertViewModel());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectUpsertViewModel model)
        {
            await LoadCategoriesAsync();

            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Projects/Create.cshtml", model);
            }

            string? coverPath = null;
            if (model.CoverImage != null)
            {
                if (!FileUploadHelper.IsValidImage(model.CoverImage, out var error))
                {
                    ModelState.AddModelError(nameof(model.CoverImage), error);
                    return View("~/Views/Admin/Projects/Create.cshtml", model);
                }

                coverPath = await FileUploadHelper.SaveFileAsync(model.CoverImage, _environment, "uploads/projects");
            }

            var project = new Project
            {
                Title = model.Title,
                Slug = await GenerateUniqueSlugAsync(model.Title),
                CategoryId = model.CategoryId,
                Location = model.Location,
                Year = model.Year,
                AreaM2 = model.AreaM2,
                Description = model.Description,
                CoverImagePath = coverPath,
                IsFeatured = model.IsFeatured,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Proje başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Edit), new { id = project.Id });
        }

        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            await LoadCategoriesAsync(project.CategoryId);

            var model = new ProjectUpsertViewModel
            {
                Id = project.Id,
                Title = project.Title,
                CategoryId = project.CategoryId,
                Location = project.Location,
                Year = project.Year,
                AreaM2 = project.AreaM2,
                Description = project.Description,
                IsFeatured = project.IsFeatured
            };

            ViewBag.Project = project;
            return View("~/Views/Admin/Projects/Edit.cshtml", model);
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectUpsertViewModel model)
        {
            var project = await _context.Projects
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            await LoadCategoriesAsync(model.CategoryId);
            ViewBag.Project = project;

            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Projects/Edit.cshtml", model);
            }

            if (model.CoverImage != null)
            {
                if (!FileUploadHelper.IsValidImage(model.CoverImage, out var error))
                {
                    ModelState.AddModelError(nameof(model.CoverImage), error);
                    return View("~/Views/Admin/Projects/Edit.cshtml", model);
                }

                FileUploadHelper.DeleteFile(_environment, project.CoverImagePath);
                project.CoverImagePath = await FileUploadHelper.SaveFileAsync(model.CoverImage, _environment, "uploads/projects");
            }

            project.Title = model.Title;
            project.Slug = await GenerateUniqueSlugAsync(model.Title, project.Id);
            project.CategoryId = model.CategoryId;
            project.Location = model.Location;
            project.Year = model.Year;
            project.AreaM2 = model.AreaM2;
            project.Description = model.Description;
            project.IsFeatured = model.IsFeatured;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Proje güncellendi.";

            return RedirectToAction(nameof(Edit), new { id = project.Id });
        }

        [HttpPost("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            if (project.Images != null)
            {
                foreach (var image in project.Images)
                {
                    FileUploadHelper.DeleteFile(_environment, image.ImagePath);
                }
            }

            FileUploadHelper.DeleteFile(_environment, project.CoverImagePath);
            _context.ProjectImages.RemoveRange(project.Images ?? new List<ProjectImage>());
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Proje silindi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("{projectId:int}/gallery")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadGallery(int projectId, List<IFormFile> images)
        {
            var project = await _context.Projects
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            if (images == null || images.Count == 0)
            {
                TempData["Error"] = "Yüklemek için en az bir görsel seçin.";
                return RedirectToAction(nameof(Edit), new { id = projectId });
            }

            var sortOrder = project.Images?.Count ?? 0;

            foreach (var file in images)
            {
                if (!FileUploadHelper.IsValidImage(file, out var error))
                {
                    TempData["Error"] = error;
                    return RedirectToAction(nameof(Edit), new { id = projectId });
                }

                var path = await FileUploadHelper.SaveFileAsync(file, _environment, "uploads/projects");
                var image = new ProjectImage
                {
                    ProjectId = projectId,
                    ImagePath = path,
                    SortOrder = sortOrder++
                };

                _context.ProjectImages.Add(image);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Galeri görselleri eklendi.";

            return RedirectToAction(nameof(Edit), new { id = projectId });
        }

        [HttpPost("gallery/delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGalleryImage(int id)
        {
            var image = await _context.ProjectImages.FindAsync(id);
            if (image == null)
            {
                return NotFound();
            }

            FileUploadHelper.DeleteFile(_environment, image.ImagePath);
            _context.ProjectImages.Remove(image);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Galeri görseli silindi.";
            return RedirectToAction(nameof(Edit), new { id = image.ProjectId });
        }

        [HttpPost("gallery/sort")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGallerySort(int projectId, int[] imageIds)
        {
            var images = await _context.ProjectImages
                .Where(i => i.ProjectId == projectId)
                .ToListAsync();

            for (var i = 0; i < imageIds.Length; i++)
            {
                var image = images.FirstOrDefault(x => x.Id == imageIds[i]);
                if (image != null)
                {
                    image.SortOrder = i;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Galeri sıralaması güncellendi.";
            return RedirectToAction(nameof(Edit), new { id = projectId });
        }

        private async Task LoadCategoriesAsync(int? selectedId = null)
        {
            var categories = await _context.PortfolioCategories
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedId);
        }

        private async Task<string> GenerateUniqueSlugAsync(string title, int? currentId = null)
        {
            var baseSlug = SlugHelper.GenerateSlug(title);
            if (string.IsNullOrWhiteSpace(baseSlug))
            {
                baseSlug = Guid.NewGuid().ToString("N");
            }

            var slug = baseSlug;
            var counter = 1;

            while (await _context.Projects.AnyAsync(p => p.Slug == slug && p.Id != currentId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }
    }
}
