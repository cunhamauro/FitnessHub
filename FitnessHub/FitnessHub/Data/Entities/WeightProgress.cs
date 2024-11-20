using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities
{
    public class WeightProgress : IEntity
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } 
        public float Weight { get; set; } 
        public DateTime Date { get; set; }
        public float Progress { get; set; }

        [Display(Name = "Image")]
        public string? ImagePath { get; set; }

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
