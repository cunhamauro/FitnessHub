using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.GymClasses
{
    public class ClassType : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Description { get; set; }

        public ClassCategory? ClassCategory { get; set; }

        public decimal Rating { get; set; }

        [Display(Name = "Reviews")]
        public int NumReviews { get; set; } = 0;

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
