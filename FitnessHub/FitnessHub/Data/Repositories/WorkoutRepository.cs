using FitnessHub.Data.Entities.GymMachines;

namespace FitnessHub.Data.Repositories
{
    public class WorkoutRepository : GenericRepository<Workout>, IWorkoutRepository
    {
        private readonly DataContext _context;

        public WorkoutRepository(DataContext context) : base(context)
        {
            _context = context;
        }
    }
}
