using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace FitnessCenterProject.Utilities
{
    public static class FileUploadHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeBytes = 2 * 1024 * 1024;

        public static bool IsValidImage(IFormFile file, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (file == null || file.Length == 0)
            {
                errorMessage = "Dosya bulunamadı.";
                return false;
            }

            if (file.Length > MaxFileSizeBytes)
            {
                errorMessage = "Dosya boyutu 2MB sınırını aşıyor.";
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                errorMessage = "Sadece JPG, PNG veya WEBP dosyaları yükleyebilirsiniz.";
                return false;
            }

            return true;
        }

        public static async Task<string> SaveFileAsync(IFormFile file, IWebHostEnvironment environment, string folder)
        {
            var uploadsRoot = Path.Combine(environment.WebRootPath, folder);
            Directory.CreateDirectory(uploadsRoot);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{folder}/{fileName}".Replace("\\", "/");
        }

        public static void DeleteFile(IWebHostEnvironment environment, string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var trimmedPath = path.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
            var fullPath = Path.Combine(environment.WebRootPath, trimmedPath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
