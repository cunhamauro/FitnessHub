using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class WorkoutViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A valid Client email is required")]
        [EmailAddress]
        public string? ClientEmail { get; set; } = string.Empty;

        public List<MachineDTO>? MachinesDTO { get; set; }

        public List<Machine>? Machines { get; set; }

        public Instructor? Instructor { get; set; }

        public List<ExerciseViewModel>? Exercises { get; set; }
    }
}
