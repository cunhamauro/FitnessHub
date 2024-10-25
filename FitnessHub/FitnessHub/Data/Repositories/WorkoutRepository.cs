using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Entities.Users;
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

        public async Task<IEnumerable<Workout>> GetAllWorkoutsIncludeAsync()
        {
            List<Workout> workouts = await _context.Workouts
                .Include(w => w.Client) 
                .Include(w => w.Instructor) 
                .Include(w => w.Exercises).ThenInclude(e => e.Machine)
                .ToListAsync();

            workouts.ForEach(workout => workout.Exercises = workout.Exercises.OrderBy(e => e.DayOfWeek).ToList());

            return workouts;
        }

        public async Task<List<Workout>> GetClientWorkoutsIncludeAsync(Client client)
        {
            return await _context.Workouts.Where(w => w.Client == client).Include(w => w.Client).Include(w => w.Instructor).Include(w => w.Exercises).ThenInclude(e => e.Machine).ToListAsync();
        }

        public async Task<Workout> GetWorkoutByIdIncludeAsync(int id)
        {
            return await _context.Workouts.Where(w => w.Id == id).Include(w => w.Client).Include(w => w.Instructor).Include(w => w.Exercises).ThenInclude(e => e.Machine).FirstOrDefaultAsync();  
        }
    }
}
