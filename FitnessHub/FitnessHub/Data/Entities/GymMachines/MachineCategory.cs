using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.GymMachines
{
    public class MachineCategory : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Description { get; set; }
    }
}
