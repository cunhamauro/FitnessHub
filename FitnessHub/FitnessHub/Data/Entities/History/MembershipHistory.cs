namespace FitnessHub.Data.Entities.History
{
    public class MembershipHistory : IEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public decimal Price { get; set; } // Monthly price

        public DateTime DateCreated { get; set; }

        public string? Description { get; set; }

        public bool Canceled { get; set; } = false;
    }
}
