using FitnessHub.Data.Entities.Users;

namespace FitnessHub.Data.Entities.GymMachines
{
    public class Workout
    {
        public int Id { get; set; }

        public Client Client { get; set; }

        public Instructor Instructor { get; set; }

        public List<Exercise> ListExercises { get; set; }
    }
}
