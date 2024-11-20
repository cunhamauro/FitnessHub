using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models.API
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [Display(Name = "Current Password")]
        public string? OldPassword { get; set; }


        [Required(ErrorMessage = "New password is required")]
        [Display(Name = "New Password")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least {2} characters long.")]
        public string? NewPassword { get; set; }


        [Required]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword")]
        public string? Confirm { get; set; }
    }
}
