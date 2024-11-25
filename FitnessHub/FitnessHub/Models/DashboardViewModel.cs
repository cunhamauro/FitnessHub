using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Models
{
    public class DashboardViewModel
    {
        [Display(Name = "Clients")]
        public int ClientsCount { get; set; }

        [Display(Name = "Clients With Membership")]
        public int ClientsWithMembershipCount { get; set; }

        [Display(Name = "Annual Memberships Revenue")]
        public decimal AnnualMembershipsRevenue { get; set; }

        [Display(Name = "Gyms")]
        public int GymsCount { get; set; }

        [Display(Name = "Instructors")]
        public int InstructorsCount { get; set; }

        [Display(Name = "Employees")]
        public int EmployeesCount { get; set; }

        [Display(Name = "Scheduled Gym Classes")]
        public int ScheduledGymClassesCount { get; set; }

        [Display(Name = "Scheduled Online Classes")]
        public int ScheduledOnlineClassesCount { get; set; }

        [Display(Name = "Video Classes")]
        public int VideoClassesCount { get; set; }

        [Display(Name = "Most Popular Class")]
        public string? MostPopularClass { get; set; }

        [Display(Name = "Gym With Most Memberships")]
        public string? GymWithMostMemberShips { get; set; }

        [Display(Name = "Countries")]
        public int CountriesCount { get; set; }
    }
}
