namespace FitnessHub.Models
{
    public class ClassDetailsViewModel
    {
        public string? InstructorName { get; set; }

        public DateTime DateStart { get; set; }

        public DateTime DateEnd { get; set; }

        public string? Location { get; set; }

        public int Id { get; set; }

        public string? Category { get; set; }

        public string? ClassType { get; set; }

        public bool IsClientRegistered { get; set; }
    }
}
