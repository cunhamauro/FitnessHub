using FitnessHub.Data.Entities.GymClasses;

namespace FitnessHub.Models
{
    public class RegisteredInClassesHistoryViewModel : RegisteredInClassesHistory
    {
        public string? CategoryName { get; set; }

        public string? TypeName { get; set; }

        public string? InstructorFullName { get; set; }

        public string? InstructorEmail { get; set; }

        public string? EmployeeFullName { get; set; }

        public string? EmployeeEmail { get; set; }

        public string? GymName { get; set; } = "N/A";

        public string? SubClass {  get; set; }

        public DateTime StartDate {  get; set; }

        public DateTime EndDate { get; set; }
    }
}
