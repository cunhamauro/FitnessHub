using FitnessHub.Data.Entities.GymMachines;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class ExerciseRepository : GenericRepository<Exercise>, IExerciseRepository
    {
        private readonly DataContext _context;

        public ExerciseRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> IsMachineIsInExercice(int id)
        {
            return await _context.Exercises.AnyAsync(ex => ex.Machine.Id == id);
        }
    }
}
