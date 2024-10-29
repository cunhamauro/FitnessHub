using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class RegisterClientInClassViewModel
    {
        [EmailAddress]
        public string? ClientEmail { get; set; }
        public int SelectedClassId { get; set; }
        public List<int> SelectedClassIds { get; set; } = new List<int>();
        public bool IsEmailValid { get; set; }

        public bool IsRegistering { get; set; } 
        public List<ClassDetailsViewModel> Classes { get; set; } = new List<ClassDetailsViewModel>();

        public class ClassDetailsViewModel
        {
            public string InstructorName { get; set; }
            public DateTime DateStart { get; set; }
            public DateTime DateEnd { get; set; }
            public string Location { get; set; }
            public int Id { get; set; }
            public string? Category { get; set; }
            public bool IsClientRegistered { get; set; }
        }
    }
}
