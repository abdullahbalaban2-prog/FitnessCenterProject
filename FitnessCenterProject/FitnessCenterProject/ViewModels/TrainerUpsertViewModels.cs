using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterProject.ViewModels
{
    public class TrainerUpsertViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(100)]
        public string? Specialty { get; set; }

        [Required]
        public int FitnessCenterId { get; set; }

        // Admin'in seçtiği hizmetler (many-to-many)
        public List<int> SelectedServiceIds { get; set; } = new();
    }
}