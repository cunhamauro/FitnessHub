using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class RequestInstructorHistoryViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Client")]
        public string? ClientName { get; set; }

        public string? ClientEmail { get; set; }

        public string? Notes { get; set; }

        [Display(Name = "Request Date")]
        public DateTime? RequestDate { get; set; }

        public string? Status { get; set; }
    }
}
