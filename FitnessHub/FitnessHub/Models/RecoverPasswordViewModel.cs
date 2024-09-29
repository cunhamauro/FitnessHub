using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class RecoverPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
