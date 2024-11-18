using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class ClientInstructorAppointmentHistoryViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Client")]
        public string? ClientName { get; set; }

        public string? ClientEmail { get; set; }

        [Display(Name = "Instructor")]
        public string? InstructorName { get; set; }

        public string? InstructorEmail { get; set; }

        [Display(Name = "Employee")]
        public string? EmployeeName { get; set; }

        public string? EmployeeEmail { get; set; }

        [Display(Name = "Assign Date")]
        public DateTime? AssignDate { get; set; }

        public string? Status { get; set; }
    }
}
