using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.History
{
    public class ClientMembershipHistory : IEntity
    {
        public int Id { get; set; } // MembershipDetails ID

        public string? UserId { get; set; }

        public bool Status { get; set; } = true;

        [Display(Name = "Date of Renewal")]
        public DateTime DateRenewal { get; set; }

        public DateTime SignUpDate { get; set; }

        public int MembershipHistoryId { get; set; }
    }
}
