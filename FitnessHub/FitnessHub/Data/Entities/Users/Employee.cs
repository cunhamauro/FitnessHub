using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.Users
{
    public class Employee : User
    {
        [Required]
        public int? GymId { get; set; }

        public Gym? Gym { get; set; }
    }
}
