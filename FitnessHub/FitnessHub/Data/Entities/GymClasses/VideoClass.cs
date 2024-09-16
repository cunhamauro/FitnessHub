using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.GymClasses
{
    public class VideoClass : Class
    {
        [Display(Name = "Video Class")]
        public string VideoClassUrl { get; set; }
    }
}
