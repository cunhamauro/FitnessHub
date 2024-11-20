using FitnessHub.Data.Entities.GymClasses;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class ClassRepository : GenericRepository<Class>, IClassRepository
    {
        private readonly DataContext _context;

        public ClassRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<OnlineClass>> GetAllOnlineClassesInclude()
        {
            return await _context.Class.OfType<OnlineClass>().Include(c => c.Category).Include(c => c.ClassType).Include(c => c.Instructor).Include(c => c.Clients).OrderBy(c => c.DateStart).ToListAsync();
        }

        public async Task<OnlineClass> GetOnlineClassByIdInclude(int id)
        {
            return await _context.Class.OfType<OnlineClass>().Where(c => c.Id == id).Include(c => c.Category).Include(c => c.ClassType).Include(c => c.Instructor).Include(c => c.Clients).FirstOrDefaultAsync();
        }

        public async Task<List<VideoClass>> GetAllVideoClassesInclude()
        {
            return await _context.Class.OfType<VideoClass>().Include(c => c.Category).Include(c => c.ClassType).ToListAsync();
        }

        public async Task<VideoClass> GetVideoClassByIdInclude(int id)
        {
            return await _context.Class.OfType<VideoClass>().Where(c => c.Id == id).Include(c => c.Category).Include(c => c.ClassType).FirstOrDefaultAsync();
        }

        public async Task<List<GymClass>> GetAllGymClassesInclude()
        {
            return await _context.Class.OfType<GymClass>().Include(c => c.Category).Include(c => c.ClassType).Include(c => c.Instructor).Include(c => c.Gym).Include(c => c.Clients).ToListAsync();
        }

        public async Task<GymClass> GetGymClassByIdInclude(int id)
        {
            return await _context.Class.OfType<GymClass>().Where(c => c.Id == id).Include(c => c.Category).Include(c => c.ClassType).Include(c => c.Instructor).Include(c => c.Clients).Include(c => c.Gym).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<GymClass?> GetGymClassByIdIncludeTracked(int id)
        {
            return await _context.Set<GymClass>()
                .Include(g => g.Clients).Include(g => g.Category).Include(g => g.ClassType).Include(g => g.Gym).Include(g => g.Instructor) // Ensure Clients are included
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<OnlineClass?> GetOnlineClassByIdIncludeTracked(int id)
        {
            return await _context.Set<OnlineClass>()
                .Include(g => g.Clients).Include(g => g.Category).Include(g => g.ClassType).Include(g => g.Instructor) // Ensure Clients are included
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<List<GymClass>> GetClassesByGym(int gymId)
        {
            var gymClasses = await _context.Class.OfType<GymClass>()
                .Where(c => c.Gym.Id == gymId && c.Clients.Count < c.Capacity)
                .Include(c => c.Gym)
                .Include(c => c.Category)
                .Include(c => c.ClassType)
                .Include(c => c.Instructor)
                .Include(c => c.Clients)
                .OrderBy(c => c.DateStart)
                .ToListAsync();

            return gymClasses;
        }
    }
}
