using FitnessHub.Data.Entities.GymMachines;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class WorkoutRepository : GenericRepository<Workout>, IWorkoutRepository
    {
        private readonly DataContext _context;

        public WorkoutRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Workout>> GetAllWorkouts()
        {
            return await _context.Workouts
                .Include(w => w.Client) 
                .Include(w => w.Instructor) 
                .Include(w => w.Exercise) 
                .ToListAsync();
        }
    }
}
