using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FitnessCenterProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Boy(cm)
        [Range(50, 250)]
        public double? Height { get; set; }

        // Kilo(kg)
        [Range(30,300)]
        public double? Weight { get; set; }

        [StringLength(50)]
        public string? BodyType { get; set; }

        [StringLength(100)]
        public string? Goal { get; set; }


        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<AiRecommendation>? AiRecommendations { get; set; }

    }
}
