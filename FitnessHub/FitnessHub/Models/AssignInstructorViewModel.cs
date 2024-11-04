using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessHub.Models
{
    public class AssignInstructorViewModel
    {
        public int RequestId { get; set; }

        public string? ClientId { get; set; }

        public string? EmployeeId { get; set; }

        public string? InstructorId { get; set; }

        public int GymId { get; set; }

        public IEnumerable<SelectListItem>? Instructors { get; set; }
    }
}
