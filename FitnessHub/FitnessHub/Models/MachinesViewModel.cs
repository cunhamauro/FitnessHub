using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class MachineViewModel
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Display(Name = "Category")]

        [Required]
        public int CategoryId { get; set; }

        public IEnumerable<SelectListItem>? Categories { get; set; }

        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Tutorial Video")]
        public string? TutorialVideoUrl { get; set; }

    }

}
