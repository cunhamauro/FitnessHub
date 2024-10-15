using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class ExerciseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter an Exercise name")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Please select a Machine")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Machine")]
        public int MachineId { get; set; }

        [Required(ErrorMessage = "Please select a duration")]
        public TimeSpan Time { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Please select the number of Exercise repetitions")]
        public int Repetitions { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Please select the number of Exercise sets")]
        public int Sets { get; set; }

        [Required(ErrorMessage = "Please select a day of the week for the Exercise")]
        public DayOfWeek DayOfWeek { get; set; }

        public string? Notes { get; set; }
    }
}
