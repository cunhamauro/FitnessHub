using FitnessHub.Data.Entities.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessHub.Data.Entities.GymClasses
{
    [NotMapped]
    public class ClassDetails
    {
        [NotMapped]
        public Client Client { get; set; }

        [NotMapped]
        public Class Class { get; set; }
    }
}
