using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class MembershipViewModel
    {
        [Display(Name = "Membership")]
        [Required]
        public int MembershipId { get; set; }

        public List<SelectListItem>? SelectMembership { get; set; }
    }
}
