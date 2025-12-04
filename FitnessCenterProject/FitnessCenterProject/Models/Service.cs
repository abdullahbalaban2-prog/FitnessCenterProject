using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterProject.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // süre(dk)
        [Range(10,300 )]
        public int DurationMinutes {  get; set; }

        // ücret
        [Column(TypeName ="decimal(10,2)")]
        [Range(0,100000)]
        public decimal Price {  get; set; }

        // Hangi salona ait
        public int FitnessCenterId { get; set; }
        public FitnessCenter? FitnessCenter { get; set; }

        public ICollection<TrainerService>? TrainerServices { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
        
    }
}
