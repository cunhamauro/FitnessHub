namespace FitnessHub.Data.Entities.Communication
{
    public class ClientInstructorAppointmentHistory : IEntity
    {
        public int Id { get; set; }

        public string? ClientId { get; set; }

        public string? EmployeeId { get; set; }

        public string? InstructorId { get; set; }

        public int GymId { get; set; }

        public DateTime? AssignDate { get; set; }
    }
}
