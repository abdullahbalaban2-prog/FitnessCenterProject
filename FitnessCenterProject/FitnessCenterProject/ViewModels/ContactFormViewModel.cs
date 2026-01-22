using System.ComponentModel.DataAnnotations;

namespace FitnessCenterProject.ViewModels
{
    public class ContactFormViewModel
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

        [StringLength(200)]
        public string? Subject { get; set; }

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;
    }
}
