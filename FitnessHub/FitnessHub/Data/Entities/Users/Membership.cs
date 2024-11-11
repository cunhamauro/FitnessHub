using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.Users
{
    public class Membership : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [DataType(DataType.Currency)]
        [Required]
        public decimal Price { get; set; } // Monthly price

        [Required]
        public string? Description {  get; set; }

        public bool? OnOffer { get; set; } = true;
    }
}
