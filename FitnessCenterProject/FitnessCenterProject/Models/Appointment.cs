using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterProject.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        // Randevuyu alan kullanıcı (üye)
        [Required]
        public string MemberId { get; set; } = null!;
        public ApplicationUser? Member { get; set; }

        // Eğitmen
        [Required]
        public int TrainerId { get; set; }
        public Trainer? Trainer { get; set; }

        // Hizmet (fitness, yoga, pilates vb.)
        [Required]
        public int ServiceId { get; set; }
        public Service? Service { get; set; }

        // Başlangıç zamanı
        [Required]
        [Display(Name = "Başlangıç Zamanı")]
        [DataType(DataType.DateTime)]
        public DateTime StartDateTime { get; set; }

        // Bitiş zamanı
        [Required]
        [Display(Name = "Bitiş Zamanı")]
        [DataType(DataType.DateTime)]
        public DateTime EndDateTime { get; set; }

        // Ücret
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Ücret")]
        public decimal Price { get; set; }

        // Onay durumu (Pending/Approved/Cancelled)
        [Required]
        [Display(Name = "Durum")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    }

}
