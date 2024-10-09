using FitnessHub.Data.Entities.GymClasses;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class GymClassViewModel : GymClass
    {
        public List<SelectListItem>? InstructorsList { get; set; }

        [Required]
        public string? InstructorId { get; set; }

        public List<SelectListItem>? GymsList { get; set; }

        [Required]
        public int GymId { get; set; }
    }
}
