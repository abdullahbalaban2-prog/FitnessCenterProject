using System.ComponentModel.DataAnnotations;

namespace FitnessCenterProject.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Bio { get; set; }

        // Uzmanlık alanı
        [StringLength(100)]
        public string? Specialty { get; set; }

        // Hangi salonda çalışıyor
        public int FitnessCenterId {  get; set; }
        public FitnessCenter? FitnessCenter { get; set; }


        // iliski
        public ICollection<TrainerService>? TrainerServices { get; set; }
        public ICollection<TrainerAvailability>? Availabilities { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }

        public string FullName => $"{FirstName} {LastName}";


    }
}
