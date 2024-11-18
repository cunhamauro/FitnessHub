using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class SendEmailViewModel
    {
        [Required(ErrorMessage = "Please write your email")]
        [EmailAddress(ErrorMessage = "Please write a valid email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Please write your name")]
        public string? Name {  get; set; }

        [Required(ErrorMessage = "Please write the message subject")]
        [MaxLength(128)]
        public string? Subject {  get; set; }

        [Required(ErrorMessage = "Please write the message body")]
        [MaxLength(512)]
        public string? Message { get; set; }
    }
}
