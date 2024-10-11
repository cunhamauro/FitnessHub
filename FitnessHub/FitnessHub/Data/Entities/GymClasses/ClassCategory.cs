namespace FitnessHub.Data.Entities.GymClasses
{
    public class ClassCategory : IEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }
    }
}
