using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterProject.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        //Randevuyu alan üye
        [Required]
        public string MemberId { get; set; } = string.Empty;
        public ApplicationUser? Member { get; set; }

        // Antrenör
        [Required]
        public int TrainerId { get; set; }
        public Trainer? Trainer { get; set; }

        // Hizmet
        [Required]
        public int ServiceId { get; set; }
        public Service? Service { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        [Column(TypeName = " decimal(10,2)")]
        [Range(0, 100000)]
        public decimal Price { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    }
}
