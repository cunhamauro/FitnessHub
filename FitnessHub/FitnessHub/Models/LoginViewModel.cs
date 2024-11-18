using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8)]
        public string? Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
