using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.Users
{
    public class Employee : User
    {
        [Required]
        public Gym? Gym { get; set; }
    }
}
