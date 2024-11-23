using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class ChangeUserViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }


        [Required]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }


        [Required]
        [Display(Name = "Birth Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime BirthDate { get; set; }


        [Display(Name = "Avatar")]
        public IFormFile? ImageFile { get; set; }


        //[Required]
        //[DataType(DataType.PhoneNumber)]
        //[Display(Name = "Phone")]
        //public string? PhoneNumber { get; set; }


        //public string? CountryCallingcode { get; set; }


        //public IEnumerable<SelectListItem>? Countries { get; set; }
    }
}
