using FitnessHub.Data.Entities.GymClasses;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class VideoClassViewModel : VideoClass
    {
        public List<SelectListItem>? CategoriesList { get; set; }

        [Required(ErrorMessage = "Please select a valid category")]
        public int CategoryId { get; set; }

        public List<SelectListItem>? ClassTypeList { get; set; }

        [Required(ErrorMessage = "Please select a valid class type")]
        public int ClassTypeId { get; set; }
    }
}
