using FitnessHub.Data.Entities.GymClasses;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class GymClassViewModel : GymClass
    {
        public List<SelectListItem>? InstructorsList { get; set; }

        [Required(ErrorMessage = "Please select a valid Instructor")]
        public string? InstructorId { get; set; }

        public List<SelectListItem>? GymsList { get; set; }

        [Required(ErrorMessage = "Please select a valid Gym")]
        public int GymId { get; set; }

        public List<SelectListItem>? CategoriesList { get; set; }

        public int CategoryId { get; set; }
    }
}
