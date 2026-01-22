using System.ComponentModel.DataAnnotations;

namespace FitnessCenterProject.Models
{
    public class Testimonial
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(160)]
        public string? CompanyOrProject { get; set; }

        [Required]
        [StringLength(1200)]
        public string Comment { get; set; } = string.Empty;

        [Range(1, 5)]
        public int? Rating { get; set; }

        public bool IsApproved { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
