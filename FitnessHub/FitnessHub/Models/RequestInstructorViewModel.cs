using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class RequestInstructorViewModel
    {
        [Required(ErrorMessage = "Please select a gym")]
        public int GymId { get; set; }

        public IEnumerable<SelectListItem>? Gyms { get; set; }

        public string? Notes {  get; set; } 
    }
}
