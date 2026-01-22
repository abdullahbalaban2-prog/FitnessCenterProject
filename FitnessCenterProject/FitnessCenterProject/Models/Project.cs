using System.ComponentModel.DataAnnotations;

namespace FitnessCenterProject.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(220)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }
        public PortfolioCategory? Category { get; set; }

        [StringLength(150)]
        public string? Location { get; set; }

        [StringLength(20)]
        public string? Year { get; set; }

        public double? AreaM2 { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(300)]
        public string? CoverImagePath { get; set; }

        public bool IsFeatured { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<ProjectImage>? Images { get; set; }
    }
}
