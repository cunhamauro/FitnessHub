using FitnessHub.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class GymViewModel : Gym
    {
        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }
    }
}
