using FitnessHub.Data.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.Communication
{
    public class Ticket : IEntity
    {
        public int Id { get; set; }

        public Client Client { get; set; }

        public DateTime DateOpened { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public bool Picked { get; set; } = false;

        public DateTime DatePicked { get; set; }

        public User? Staff { get; set; }

        public List<TicketMessage>? TicketMessages { get; set; }

        public bool HasStaffReply { get; set; } = false;

        public bool HasClientReply { get; set; } = false;

        public bool Open { get; set; } = true;
    }
}
