using FitnessHub.Data.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.Communication
{
    public class RequestInstructor : IEntity
    {
        public int Id { get; set; }

        [Required]
        public Client? Client { get; set; }

        [Required]
        public int GymId { get; set; }

        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Request Date")]
        public DateTime? RequestDate { get; set; } = DateTime.UtcNow;
    }
}
