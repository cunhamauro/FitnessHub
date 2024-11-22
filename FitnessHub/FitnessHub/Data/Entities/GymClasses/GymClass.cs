using FitnessHub.Data.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FitnessHub.Data.Entities.GymClasses
{
    public class GymClass : Class
    {
        public Gym? Gym { get; set; }

        public Instructor? Instructor { get; set; }

        [Display(Name = "Start Date")]
        public DateTime DateStart { get; set; }

        [Display(Name = "End Date")]
        public DateTime DateEnd { get; set; }

        [JsonIgnore]
        public List<Client>? Clients { get; set; }

        public int Capacity { get; set; }

        //public List<ClassDetails> ClassDetails { get; set; } = new();
    }
}
