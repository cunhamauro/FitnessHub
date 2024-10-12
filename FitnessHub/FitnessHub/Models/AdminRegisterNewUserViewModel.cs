﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessHub.Models
{
    public class AdminRegisterNewUserViewModel
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


        public IEnumerable<SelectListItem>? Gyms { get; set; }


        [Required]
        [Display(Name = "Selected Gym")]
        public int SelectedGym { get; set; }


        [Required]
        public string? SelectedRole { get; set; }


        public IEnumerable<SelectListItem>? Roles { get; set; }


        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
    }
}
