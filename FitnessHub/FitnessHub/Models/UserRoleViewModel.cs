using System.ComponentModel.DataAnnotations;
using FitnessHub.Data.Entities;

namespace FitnessHub.Models
{
    public class UserRoleViewModel
    {
        public string? Id { get; set; }


        [Display(Name = "First Name")]
        public string? FirstName { get; set; }


        [Display(Name = "Last Name")]
        public string? LastName { get; set; }


        public string? Email { get; set; }


        public Gym? Gym { get; set; }


        public string? Role { get; set; }


        [Display(Name = "Name")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Image")]
        public string? Avatar { get; set; }
    }
}
