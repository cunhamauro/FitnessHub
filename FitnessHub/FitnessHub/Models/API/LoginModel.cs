using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models.API
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string? Email { get; set; }


        [MinLength(8)]
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}
