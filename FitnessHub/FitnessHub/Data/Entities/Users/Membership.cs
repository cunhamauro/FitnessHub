using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.Users
{
    public class Membership : IEntity
    {
        public int Id { get; set; }

        [Range(0, 3)]
        [Required]
        public int Tier {  get; set; } // 0, 1, 2, etc...

        [Required]
        [Display(Name = "Membership Type")]
        public string? Type { get; set; }

        [Required]
        public decimal Price { get; set; } // Monthly price
    }
}
