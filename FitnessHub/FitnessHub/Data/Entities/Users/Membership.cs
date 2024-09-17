using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.Users
{
    public class Membership : IEntity
    {
        public int Id { get; set; }

        public bool Status { get; set; }

        [Display(Name = "Date of Renewal")]
        public DateTime DateRenewal { get; set; }

        [Display(Name = "Membership Tier")]
        public int Tier { get; set; }
    }
}
