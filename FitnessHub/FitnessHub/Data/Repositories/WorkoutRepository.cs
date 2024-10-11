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

        public async Task<IEnumerable<Workout>> GetAllWorkoutsInclude()
        {
            return await _context.Workouts
                .Include(w => w.Client) 
                .Include(w => w.Instructor) 
                .Include(w => w.Exercises).ThenInclude(e => e.Machine)
                .ToListAsync();
        }

        public async Task<Workout> GetWorkoutByIdIncludeAsync(int id)
        {
            return await _context.Workouts.Where(w => w.Id == id).Include(w => w.Client).Include(w => w.Instructor).Include(w => w.Exercises).ThenInclude(e => e.Machine).FirstOrDefaultAsync();  
        }
    }
}
