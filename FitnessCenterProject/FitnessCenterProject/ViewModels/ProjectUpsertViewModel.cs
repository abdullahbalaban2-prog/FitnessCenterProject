using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FitnessCenterProject.ViewModels
{
    public class ProjectUpsertViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [StringLength(150)]
        public string? Location { get; set; }

        [StringLength(20)]
        public string? Year { get; set; }

        public double? AreaM2 { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        public bool IsFeatured { get; set; }

        public IFormFile? CoverImage { get; set; }
    }
}
