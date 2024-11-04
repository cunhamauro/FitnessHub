using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class MyRequestInstructorHistoryViewModel
    {
        public string? Gym { get; set; }

        public string? Notes { get; set; }

        [Display(Name = "Request Date")]
        public DateTime? RequestDate { get; set; }

        public string? Status { get; set; }
    }
}
