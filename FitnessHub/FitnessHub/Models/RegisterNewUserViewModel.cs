﻿using FitnessHub.Data.HelperClasses;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class RegisterNewUserViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }


        [Required]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }


        [Required]
        [AgeValidation]
        [Display(Name = "Birth Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime BirthDate { get; set; }


        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }


        [Required]
        [MinLength(8)]
        public string? Password { get; set; }


        [Required]
        [Compare("Password")]
        public string? Confirm { get; set; }
    }
}
