using FitnessHub.Data.Entities.Users;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class MembershipViewModel
    {
        [Display(Name = "Membership")]
        [Required]
        public int MembershipId { get; set; }

        public List<SelectListItem>? SelectMembership { get; set; }

        [Required]
        [Length(minimumLength:8,maximumLength:15, ErrorMessage = "The Identification Number must contain between {1} and {2} digits")]
        [Display(Name = "Identification Number")]
        public string? IdNumber { get; set; }

        [Required]
        [Length(minimumLength:5,maximumLength:100, ErrorMessage = "The Address must have a length between {1} and {2} characters")]
        [Display(Name = "Full Address")]
        public string? FullAddress { get; set; }
    }
}
