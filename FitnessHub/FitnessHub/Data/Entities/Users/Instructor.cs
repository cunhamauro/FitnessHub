using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.Users
{
    public class Instructor : User
    {
        [Required]
        public int? GymId { get; set; }

        public Gym? Gym { get; set; }

        [Range(1,5)]
        public decimal Rating {  get; set; }

        [Display(Name = "Reviews")]
        public int NumReviews { get; set; } = 0;
    }
}
