using FitnessHub.Data.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessHub.Data.Entities.Communication
{
    public class ForumThread : IEntity
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        [NotMapped]
        public bool Owner { get; set; } = false;

        public User User { get; set; }

        public string UserRole { get; set; }

        [Required(ErrorMessage = "Please write or select a Post Theme")]
        public string? Theme { get; set; }

        [Required(ErrorMessage = "Please write a Post Title")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Please write a Post Message")]
        public string? Message { get; set; }

        public List<ForumReply> ForumReplies { get; set; }

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
