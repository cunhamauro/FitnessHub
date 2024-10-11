using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.GymMachines
{
    public class Machine : IEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }
        public MachineCategory? Category { get; set; }

        [Display(Name = "Image")]
        public string? ImagePath { get; set; }

        [Display(Name = "Tutorial Video")]
        public string? TutorialVideoUrl { get; set; }
    }
}
