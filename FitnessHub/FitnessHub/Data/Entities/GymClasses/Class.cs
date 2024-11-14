namespace FitnessHub.Data.Entities.GymClasses
{
    public class Class : IEntity
    {
        public int Id { get; set; }

        public ClassCategory? Category { get; set; }

        public ClassType? ClassType { get; set; }
    }
}
