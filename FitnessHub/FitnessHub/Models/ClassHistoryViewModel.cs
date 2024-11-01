using FitnessHub.Data.Entities.History;

namespace FitnessHub.Models
{
    public class ClassHistoryViewModel : ClassHistory
    {
        public string? CategoryName { get; set; }

        public string? InstructorFullName { get; set; } = "N/A";

        public string? InstructorEmail { get; set; } = "N/A";

        public string? GymName { get; set; } = "N/A";

        public List<string>? ClientList { get; set; }
    }
}
