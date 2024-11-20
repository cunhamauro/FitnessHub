using FitnessHub.Data.Entities.Users;

namespace FitnessHub.Data.Entities.GymMachines
{
    public class Workout : IEntity
    {
        public int Id { get; set; }

        public Client? Client { get; set; }

        public Instructor? Instructor { get; set; }

        public List<Exercise>? Exercises { get; set; }

        public DateTime DateModified { get; set; }
    }
}
