using FitnessHub.Data.Entities.Users;

namespace FitnessHub.Data.Entities.Communication
{
    public class TicketMessage : IEntity
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public User User { get; set; }

        public string Message { get; set; }

        public string UserRole { get; set; }
    }
}
