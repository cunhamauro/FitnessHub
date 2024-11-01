using FitnessHub.Data.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.GymClasses
{
    public class OnlineClass : Class
    {
        public Instructor? Instructor {  get; set; }

        [Required]
        [Display(Name = "Date Start")]
        public DateTime DateStart {  get; set; }

        [Required]
        [Display(Name = "Date End")]
        public DateTime DateEnd { get; set; }

        [Required]
        public string? Platform {  get; set; }

        public List<Client>? Clients { get; set; }

        //public List<ClassDetails> ClassDetails { get; set; } = new();

    }
}
