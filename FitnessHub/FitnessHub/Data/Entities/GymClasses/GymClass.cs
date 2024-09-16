using FitnessHub.Data.Entities.Users;

namespace FitnessHub.Data.Entities.GymClasses
{
    public class GymClass : Class
    {
        public Gym Gym { get; set; }

        public Instructor Instructor { get; set; }

        public DateTime DateStart { get; set; }

        public DateTime DateEnd { get; set; }

        public List<Client> ListClients { get; set; }
    }
}
