namespace FitnessHub.Data.Entities.GymMachines
{
    public class MachineCategory : IEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }
    }
}
