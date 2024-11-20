namespace FitnessHub.Data.Entities.History
{
    public class ClassHistory : IEntity
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Category { get; set; }

        public string? ClassType { get; set; }

        public int? GymId {  get; set; }

        /// <summary>
        /// Only for GymClass
        /// </summary>
        public string? GymName { get; set; } = "N/A";

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
        public string? Platform { get; set; } = "N/A";

        /// <summary>
        /// Only for VideoClass
        /// </summary>
        public string? VideoClassUrl { get; set; }

        public bool Canceled { get; set; } = false;

        public string? SubClass { get; set; }
    }
}
