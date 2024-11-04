using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

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
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime BirthDate { get; set; }

        [Display(Name = "Profile Image")]
        public string? ImagePath { get; set; }

        [Display(Name = "Image")]
        public string Avatar
        {
            get
            {
                if (string.IsNullOrEmpty(ImagePath))
                {
                    return $"/images/defaultavatar.png";
                }
                else
                {
                    return $"{ImagePath.Substring(1)}";
                }
            }
        }

        [Display(Name = "Name")]
        public string? FullName => $"{FirstName} {LastName}";
    }
}
