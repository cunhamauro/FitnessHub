namespace FitnessHub.Data.Entities.GymMachines
{
    public class Exercise : IEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public Machine? Machine { get; set; }

        // TODO: Convert TimeSpan to Ticks in Controller/Action
        public long Ticks { get; set; } // Save TimeSpan has Ticks to SQL Server (TimeSpan > 23.59 is not supported)

        public TimeSpan Time => TimeSpan.FromTicks(Ticks); // Convert the Ticks from SQL Server to TimeSpan

        public int Repetitions { get; set; }

        public int Sets { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public string? Notes { get; set; }
    }
}
