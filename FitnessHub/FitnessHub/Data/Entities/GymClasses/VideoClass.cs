using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.GymClasses
{
    public class VideoClass : Class
    {
        [Required]
        [Display(Name = "Video")]
        public string? VideoClassUrl { get; set; }

        [Required]
        public string? Title {  get; set; }

        [Required]
        public string? Description { get; set; }
    }
}
