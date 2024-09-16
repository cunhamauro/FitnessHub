using FitnessHub.Data.Entities.GymClasses;

namespace FitnessHub.Data.Entities.Users
{
    public class Client : User
    {
        public Membership Membership {  get; set; }

        public List<OnlineClass> OnlineClass {  get; set; }

        public List<GymClass> GymClass { get; set; }

    }
}
