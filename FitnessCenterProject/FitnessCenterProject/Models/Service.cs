using System.ComponentModel.DataAnnotations;

namespace FitnessCenterProject.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(80)]
        public string? IconName { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }
    }
}
