using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.GymMachines
{
    public class MachineDetails : IEntity
    {
        public int Id { get; set; }

        public Machine? Machine { get; set; }

        public Gym? Gym { get; set; }

        public bool Status { get; set; }

        [Display(Name ="Status")]
        public string StatusDisplay => Status ? "Available" : "Unavailable";

        [Display(Name = "ID No.")]
        public int MachineNumber { get; set; }
    }
}
