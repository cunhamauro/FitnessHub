namespace FitnessHub.Models.API
{
    public class ExerciseModel
    {
        public string ExerciseName { get; set; }

        public string MachineName { get; set; }

        public TimeSpan Time {  get; set; }

        public int Repetitions {  get; set; }

        public int Sets {  get; set; }

        public DayOfWeek DayOfWeek {  get; set; }

        public string Notes { get; set; }
    }
}
