using System.ComponentModel.DataAnnotations;
using FitnessHub.Data.HelperClasses;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessHub.Models
{
    public class AdminRegisterNewUserViewModel
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
        [AgeValidation]
        public DateTime BirthDate { get; set; }


        public IEnumerable<SelectListItem>? Gyms { get; set; }


        [Required]
        public int Gym { get; set; }


        [Required]
        public string? Role { get; set; }


        public IEnumerable<SelectListItem>? Roles { get; set; }


        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }


        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone")]
        public string? PhoneNumber { get; set; }


        public string? CountryCallingcode { get; set; }


        public IEnumerable<SelectListItem>? Countries { get; set; }
    }
}
