using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class EmployeeRegisterNewClientViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }


        [Required]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }


        [Required]
        [Display(Name = "Birth Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime BirthDate { get; set; }


        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
    }
}
