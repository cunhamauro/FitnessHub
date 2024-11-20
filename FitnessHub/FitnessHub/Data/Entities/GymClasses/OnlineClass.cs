using FitnessHub.Data.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FitnessHub.Data.Entities.GymClasses
{
    public class OnlineClass : Class
    {
        public Instructor? Instructor {  get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime DateStart {  get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime DateEnd { get; set; }

        [Required]
        public string? Platform {  get; set; }

        [JsonIgnore]
        public List<Client>? Clients { get; set; }

        //public List<ClassDetails> ClassDetails { get; set; } = new();
    }
}
