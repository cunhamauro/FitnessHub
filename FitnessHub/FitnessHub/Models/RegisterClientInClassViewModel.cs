using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class RegisterClientInClassViewModel
    {
        [EmailAddress]
        public string? ClientEmail { get; set; }

        public string? ClientFullName {  get; set; }

        public int SelectedClassId { get; set; }

        public List<int> SelectedClassIds { get; set; } = new List<int>();

        public bool IsEmailValid { get; set; }

        public bool IsRegistering { get; set; }

        public List<ClassDetailsViewModel> Classes { get; set; } = new List<ClassDetailsViewModel>();
    }
}
