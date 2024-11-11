using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.GymClasses
{
    public class Class : IEntity
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        [Display(Name = "Reviews")]
        public int? NumReviews { get; set; } = 0;

        public ClassCategory? Category { get; set; }

        public ClassType? ClassType { get; set; }
    }
}
