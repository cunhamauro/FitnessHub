using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class ClassesDetailsViewModel
    {
        public string? InstructorName { get; set; }

        public DateTime DateStart { get; set; }

        public DateTime DateEnd { get; set; }

        public string? Location { get; set; }

        public string? Category { get; set; }

        public string? ClassType { get; set; }

        public string? GymName { get; set; }

        public string? Platform { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        [Display(Name = "Reviews")]
        public int? NumReviews { get; set; } = 0;
    }
}
