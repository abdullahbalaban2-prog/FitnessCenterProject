using System.ComponentModel.DataAnnotations;

namespace FitnessCenterProject.Models
{
    public class PortfolioCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string Slug { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public ICollection<Project>? Projects { get; set; }
    }
}
