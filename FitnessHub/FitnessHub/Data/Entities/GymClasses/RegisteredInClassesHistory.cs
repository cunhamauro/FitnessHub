namespace FitnessHub.Data.Entities.GymClasses
{
    public class RegisteredInClassesHistory : IEntity
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public int ClassId { get; set; }

        public string? EmployeeId { get; set; }

        public bool Canceled { get; set; } = false;

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public bool Reviewed { get; set; } = false;

        public decimal Rating { get; set; }
    }
}
