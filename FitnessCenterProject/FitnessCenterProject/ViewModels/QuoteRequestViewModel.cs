using System.ComponentModel.DataAnnotations;

namespace FitnessCenterProject.ViewModels
{
    public class QuoteRequestViewModel
    {
        [Required]
        [StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(160)]
        public string Email { get; set; } = string.Empty;

        [StringLength(40)]
        public string? Phone { get; set; }

        [Required]
        [StringLength(120)]
        public string ProjectType { get; set; } = string.Empty;

        [StringLength(120)]
        public string? BudgetRange { get; set; }

        public DateTime? DesiredDate { get; set; }

        [StringLength(2000)]
        public string? Message { get; set; }
    }
}
