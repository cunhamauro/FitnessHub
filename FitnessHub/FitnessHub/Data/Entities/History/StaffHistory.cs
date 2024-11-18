using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.History
{
    public class StaffHistory
    {
        public int Id { get; set; }
        public string? StaffId { get; set; }

        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "Birth Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime BirthDate { get; set; }

        [Display(Name = "Phone")]
        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        [Display(Name = "Gym")]
        public int GymId { get; set; }

        public string? Role { get; set; }
    }
}
