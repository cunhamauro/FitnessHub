using FitnessHub.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class WeightProgressViewModel : WeightProgress
    {
        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }
    }
}
