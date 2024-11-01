using FitnessHub.Data.Entities.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessHub.Data.Entities.GymClasses
{
    public class ClassDetails
    {
        public string ClientId { get; set; }
        public Client Client { get; set; }

        public int ClassId {  get; set; }
        public Class Class { get; set; }

        // Additional property to help EF Core identify the class type
        public string ClassType { get; set; } // Store the type of class (GymClass, OnlineClass)
    }
}
