using FitnessHub.Data.HelperClasses;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models.API
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Birth Date is required")]
        [AgeValidation]
        [Display(Name = "Birth Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime BirthDate { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Gym is required")]
        public int GymId { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least {2} characters long.")]
        public string? Password { get; set; }

        [Required]
        [Compare("Password")]
        public string? Confirm { get; set; }
    }
}
