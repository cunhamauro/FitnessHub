using FitnessHub.Data.Entities.History;

namespace FitnessHub.Models
{
    public class ClassHistoryViewModel : ClassHistory
    {
        public List<string>? ClientList { get; set; }

        public string? InstructorFullName {  get; set; }

        public string? InstructorEmail { get; set; }
    }
}
