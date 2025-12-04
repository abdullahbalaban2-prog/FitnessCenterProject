using System.ComponentModel.DataAnnotations;

namespace FitnessCenterProject.Models
{
    public class TrainerAvailability
    {
        public int Id { get; set; } 

        public int TrainerId {  get; set; }
        public Trainer? Trainer { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

    }
}
