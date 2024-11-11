namespace FitnessHub.Data.Entities.History
{
    public class ClassHistory : IEntity
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public int ClassTypeId { get; set; }

        /// <summary>
        /// Only for GymClass
        /// </summary>
        public int? GymId { get; set; }  

        /// <summary>
        /// Only for GymClass & OnlineClass
        /// </summary>
        public string? InstructorId { get; set; }

        /// <summary>
        /// Only for GymClass & OnlineClass
        /// </summary>
        public DateTime? DateStart { get; set; }

        /// <summary>
        /// Only for GymClass & OnlineClass
        /// </summary>
        public DateTime? DateEnd { get; set; }

        /// <summary>
        /// Only for GymClass
        /// </summary>
        public int? Capacity { get; set; }

        /// <summary>
        /// Only for OnlineClass
        /// </summary>
        public string? Platform { get; set; }

        /// <summary>
        /// Only for VideoClass
        /// </summary>
        public string? VideoClassUrl { get; set; }

        public bool Canceled { get; set; } = false;

        public string? SubClass { get; set; }
    }
}
