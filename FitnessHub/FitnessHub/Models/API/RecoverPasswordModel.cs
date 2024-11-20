using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models.API
{
    public class RecoverPasswordModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
