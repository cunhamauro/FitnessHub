using FitnessHub.Data.Entities.GymClasses;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class OnlineClassViewModel : OnlineClass
    {
        public List<SelectListItem>? InstructorsList { get; set; }

        public List<SelectListItem>? CategoriesList { get; set; }

        [Required]
        public string? InstructorId {  get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}
