using FitnessHub.Data.Entities.GymMachines;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class WorkoutViewModel
    {
        public int Id { get; set; }

        // O campo para o email do cliente
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? ClientEmail { get; set; } = string.Empty;
        public List<SelectListItem>? Machines { get; set; }
        public string? InstructorFullName { get; set; }
        //public List<Exercise>? Exercises { get; set; }
    }
}
