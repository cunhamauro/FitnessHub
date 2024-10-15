using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.GymMachines
{
    public class Machine : IEntity
    {
        public int Id { get; set; }

        [Display(Name = "Machine")]
        public string? Name { get; set; }

        public MachineCategory? Category { get; set; }

        [Display(Name = "Image")]
        public string? ImagePath { get; set; }

        [Display(Name = "Video")]
        public string? TutorialVideoUrl { get; set; }

        [Display(Name = "Image")]
        public string ImageDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(ImagePath))
                {
                    return $"/images/noimage.png";
                }
                else
                {
                    return $"{ImagePath.Substring(1)}";
                }
            }
        }
    }
}
