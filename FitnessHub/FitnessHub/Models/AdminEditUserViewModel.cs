using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class AdminEditUserViewModel
    {
        public string? Id { get; set; }


        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }


        public IEnumerable<SelectListItem>? Gyms { get; set; }


        [Required]
        [Display(Name = "Selected Gym")]
        public int GymId { get; set; }
    }
}
