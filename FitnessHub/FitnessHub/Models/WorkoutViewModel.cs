using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Entities.Users;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class WorkoutViewModel
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string? ClientEmail { get; set; } = string.Empty;

        public List<SelectListItem>? Machines { get; set; }

        public Instructor Instructor { get; set; }

        public List<ExerciseViewModel>? Exercises { get; set; }
    }
}
