using FitnessHub.Data.Entities.GymClasses;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class ClassTypeViewModel : ClassType
    {
        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }

        public List<SelectListItem>? SelectListCategories { get; set; }

        public int CategoryId { get; set; }
    }
}
