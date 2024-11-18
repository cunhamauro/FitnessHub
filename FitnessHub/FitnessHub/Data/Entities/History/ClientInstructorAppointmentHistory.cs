namespace FitnessHub.Data.Entities.History
{
    public class ClientInstructorAppointmentHistory : IEntity
    {
        public int Id { get; set; }

        public string? ClientId { get; set; }

        public string? EmployeeId { get; set; }

        public string? InstructorId { get; set; }

        public int GymId { get; set; }

        public DateTime? AssignDate { get; set; }

        public bool IsResolved { get; set; } 
    }
}
