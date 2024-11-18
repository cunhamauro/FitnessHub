using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid e-mail address")]
        public string? Username { get; set; }


        [Required]
        public string? Password { get; set; }


        public bool RememberMe { get; set; }
    }
}
