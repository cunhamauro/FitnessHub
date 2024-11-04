using FitnessHub.Data.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.Communication
{
    public class ClientInstructorAppointment : IEntity
    {
        public int Id { get; set; }

        [Required]
        public Client? Client { get; set; }

        [Required]
        public string? EmployeeId { get; set; }

        [Required]
        public string? InstructorId { get; set; }

        [Required]
        [Display(Name = "Assign Date")]
        public DateTime? AssignDate { get; set; } = DateTime.UtcNow;
    }
}
