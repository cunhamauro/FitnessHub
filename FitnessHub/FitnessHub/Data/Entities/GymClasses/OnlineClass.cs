using FitnessHub.Data.Entities.Users;

namespace FitnessHub.Data.Entities.GymClasses
{
    public class OnlineClass : Class
    {
        public Instructor Instructor {  get; set; }

        public DateTime DateStart {  get; set; }

        public DateTime DateEnd { get; set; }

        public string Platform {  get; set; }

        public List<Client> Client { get; set; }
    }
}
