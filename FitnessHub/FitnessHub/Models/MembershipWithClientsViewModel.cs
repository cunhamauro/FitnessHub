using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class MembershipWithClientsViewModel
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [DataType(DataType.Currency)]
        [Required]
        [Display(Name = "Monthly Fee")]
        public decimal MonthlyFee { get; set; } // Monthly price

        [Display(Name = "Annual")]
        public decimal AnnualFee => MonthlyFee * 12;

        [Required]
        public string? Description { get; set; }

        public bool? OnOffer { get; set; } = true;

        public int Clients { get; set; }
    }
}
