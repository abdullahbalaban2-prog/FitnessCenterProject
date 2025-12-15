using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterProject.Models
{
    public class AiRecommendation
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        // Navigation (Admin listesinde x.User?.Email için gerekli)
        public ApplicationUser? User { get; set; }

        public int Height { get; set; }
        public int Weight { get; set; }

        public string? BodyType { get; set; }
        public string? Goal { get; set; }
        public string? RequestType { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string GeneratedPlan { get; set; } = string.Empty;

        public string? GeneratedImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
