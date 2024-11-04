using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.History
{
    public class GymHistory : IEntity
    {
        public int Id { get; set; }

        [Display(Name = "Gym")]
        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Country { get; set; }

        [Required]
        public string? City { get; set; }

        [Required]
        public string? Address { get; set; }
    }
}
