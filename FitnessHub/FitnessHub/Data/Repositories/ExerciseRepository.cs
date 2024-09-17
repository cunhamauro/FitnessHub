using FitnessHub.Data.Entities.GymMachines;

namespace FitnessHub.Data.Repositories
{
    public class ExerciseRepository : GenericRepository<Exercise>, IExerciseRepository
    {
        private readonly DataContext _context;

        public ExerciseRepository(DataContext context) : base(context)
        {
            _context = context;
        }
    }
}
