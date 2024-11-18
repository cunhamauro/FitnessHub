using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class StaffHistoryViewModel
    {
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "Birth Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime BirthDate { get; set; }

        public string? Email { get; set; }

        public string? Gym { get; set; }

        public string? Role { get; set; }

        [Display(Name = "Phone")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Name")]
        public string FullName => $"{FirstName} {LastName}";
    }
}
