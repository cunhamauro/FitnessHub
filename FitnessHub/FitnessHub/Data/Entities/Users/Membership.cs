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
        public decimal MonthlyFee { get; set; } // Monthly price

        public decimal AnnualFee => MonthlyFee * 12;

        [Required]
        public string? Description {  get; set; }

        public bool? OnOffer { get; set; } = true;
    }
}
