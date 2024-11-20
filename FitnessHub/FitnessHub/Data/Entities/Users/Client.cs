using FitnessHub.Data.Entities.GymClasses;

namespace FitnessHub.Data.Entities.Users
{
    public class Client : User
    {
        public MembershipDetails? MembershipDetails {  get; set; }

        public int? MembershipDetailsId { get; set; }

        public List<OnlineClass>? OnlineClass { get; set; }

        public List<GymClass>? GymClass { get; set; }

        public int GymId { get; set; }

        public Gym? Gym { get; set; }

        //public List<ClassDetails>? ClassDetails { get; set; } = new();
    }
}
