namespace FitnessHub.Data.Entities.Users
{
    public class Employee : User
    {
        public int GymId { get; set; }

        public Gym? Gym { get; set; }
    }
}
