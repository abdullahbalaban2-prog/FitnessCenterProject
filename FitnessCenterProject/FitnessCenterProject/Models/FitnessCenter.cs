using System.ComponentModel.DataAnnotations;

namespace FitnessCenterProject.Models
{

    public class FitnessCenter
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }


        [StringLength(50)]
        public string? WorkingHours { get; set; }

        // İlişkiler: 1 salonun birden çok hizmeti ve antrenörü olabilir
        public ICollection<Service>? Services { get; set; }
        public ICollection<Trainer>? Trainers { get; set; }
    }
}