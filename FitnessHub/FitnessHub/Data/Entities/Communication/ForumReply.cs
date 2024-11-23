using FitnessHub.Data.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessHub.Data.Entities.Communication
{
    public class ForumReply : IEntity
    {
        public int Id { get; set; }

        public int NavigatePost {  get; set; }

        public DateTime Date { get; set; }

        public User User { get; set; }

        public string UserRole {  get; set; }

        public string? Message { get; set; }

        [NotMapped]
        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }

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
