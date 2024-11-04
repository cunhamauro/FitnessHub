using FitnessHub.Data.Entities.History;

namespace FitnessHub.Models
{
    public class ClientMembershipHistoryViewModel : ClientMembershipHistory
    {
        public string? Name { get; set; }

        public decimal Price { get; set; } // Monthly price
    }
}
