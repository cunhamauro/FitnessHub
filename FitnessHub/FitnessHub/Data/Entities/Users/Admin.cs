namespace FitnessHub.Data.Entities.Users
{
    public class Admin : User
    {
        public int? GymId { get; set; }

        public Gym? Gym { get; set; }
    }
}

