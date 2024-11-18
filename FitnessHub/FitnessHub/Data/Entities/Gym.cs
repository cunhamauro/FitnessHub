using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities
{
    public class Gym : IEntity
    {
        public int Id { get; set; }

        [Display(Name = "Gym")]
        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Country { get; set; }

        public string? FlagUrl {  get; set; }

        [Required]
        public string? City { get; set; }

        [Required]
        public string? Address { get; set; }

        public string Data => $"{Name} - {Address}, {City}, {Country}";

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
