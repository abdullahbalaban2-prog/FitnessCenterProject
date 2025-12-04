using System.ComponentModel.DataAnnotations;

namespace FitnessCenterProject.Models
{
    public class AiRecommendation
    {
        public int Id { get; set; }

        [Required]
        public string UserId {  get; set; }=string.Empty;
        public ApplicationUser? User { get; set; }
        
        public double? Height { get; set; }
        public double? Weight{ get; set; }

        [StringLength(50)]
        public string? BodyType { get; set; }   

        [StringLength(50)]
        public string? Goal{ get; set; }
        
        [StringLength(50)]
        public string? RequestType {  get; set; }

        // AI trf. üretilen plan
        [StringLength(50)]
        public string? GeneratedPlan{ get; set; }

        [StringLength(500)]
        public string? GeneratedImageUrl {  get; set; }   

        public DateTime CreatedAt {  get; set; } = DateTime.UtcNow;

    }
}
