using System.ComponentModel.DataAnnotations;

namespace FitnessCenterProject.Models
{
    public class ProjectImage
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        [Required]
        [StringLength(300)]
        public string ImagePath { get; set; } = string.Empty;

        public int SortOrder { get; set; }
    }
}
