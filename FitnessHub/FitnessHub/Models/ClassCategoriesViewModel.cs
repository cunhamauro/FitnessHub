using FitnessHub.Data.Entities.GymClasses;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class ClassCategoriesViewModel : ClassCategory
    {
        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }
    }
}
