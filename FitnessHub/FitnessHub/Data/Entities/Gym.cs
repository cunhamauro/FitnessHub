using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities
{
    public class Gym : IEntity
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

        public int Rating {  get; set; }

        [Display(Name = "Reviews")]
        public int NumReviews { get; set; }

        public string Data => $"{Name} - {Address}, {City}, {Country}";
    }
}
