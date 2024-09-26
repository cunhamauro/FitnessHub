using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string? Email { get; set; }


        [Required]
        public string? Token { get; set; }


        [Required]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }


        [Required]
        [Compare("NewPassword")]
        public string? Confirm { get; set; }
    }
}
