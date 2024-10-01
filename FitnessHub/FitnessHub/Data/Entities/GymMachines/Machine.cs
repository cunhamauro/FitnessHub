using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.GymMachines
{
    public class Machine : IEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public Category? Category { get; set; }

        public int CategoryId { get; set; }

        [Display(Name = "Image")]
        public string? ImagePath { get; set; }

        [Display(Name = "Tutorial Video")]
        public string? TutorialVideoUrl { get; set; }
    }
}
