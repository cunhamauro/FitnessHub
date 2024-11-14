using FitnessHub.Data.Entities.GymClasses;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class GymClassViewModel : GymClass
    {
        public List<SelectListItem>? InstructorsList { get; set; }

        [Required(ErrorMessage = "Please select a valid instructor")]
        public string? InstructorId { get; set; }

        [Required(ErrorMessage = "Please select a valid gym")]
        public int GymId { get; set; }

        public List<SelectListItem>? CategoriesList { get; set; }

        public List<SelectListItem>? ClassTypeList { get; set; }

        public int CategoryId { get; set; }

        public int ClassTypeId { get; set; }
    }
}
