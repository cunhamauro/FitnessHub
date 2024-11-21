namespace FitnessHub.Models
{
    public class ClassViewModel
    {
        public string? InstructorName { get; set; }

        public DateTime DateStart { get; set; }

        public DateTime DateEnd { get; set; }

        public string? Location { get; set; }

        public bool IsOnline { get; set; }

        public int Id { get; set; }

        public string? Category { get; set; }

        public string? ClassType {  get; set; }

        public bool? Full {  get; set; }

        public bool? InWaitlist {  get; set; }

        public bool? Registered {  get; set; }
    }
}
