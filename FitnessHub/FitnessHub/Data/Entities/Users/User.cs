using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.Users
{
    public class User : IdentityUser
    {
        [MaxLength(50, ErrorMessage = "The {0} can only contain {1} characters!")]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [MaxLength(50, ErrorMessage = "The {0} can only contain {1} characters!")]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "Birth Date")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "Profile Image")]
        public string? ImagePath { get; set; }
    }
}
