using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities
{
    public class Gym : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public int Rating {  get; set; }

        [Display(Name = "Reviews")]
        public int NumReviews { get; set; }
    }
}
