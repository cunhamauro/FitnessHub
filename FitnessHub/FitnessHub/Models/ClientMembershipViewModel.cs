using FitnessHub.Data.Entities.Users;

namespace FitnessHub.Models
{
    public class ClientMembershipViewModel : MembershipDetails
    {
        public string? ClientEmail { get; set; }

        public string? ClientFullName { get; set; }
    }
}
