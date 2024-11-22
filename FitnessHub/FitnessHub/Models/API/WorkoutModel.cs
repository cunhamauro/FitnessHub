namespace FitnessHub.Models.API
{
    public class WorkoutModel
    {
        public string? InstructorEmail { get; set; }

        public string? InstructorName { get; set; }

        public List<ExerciseModel>? ExerciseList {  get; set; } = new List<ExerciseModel>();

        public DateTime Date { get; set; }
    }
}
