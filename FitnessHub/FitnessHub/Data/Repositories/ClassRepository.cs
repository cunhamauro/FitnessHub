using FitnessHub.Data.Entities.GymClasses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

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
            return await _context.Class.OfType<OnlineClass>().Include(c => c.Category).Include(c => c.Instructor).Include(c => c.Clients).ToListAsync();
        }

        public async Task<OnlineClass> GetOnlineClassByIdInclude(int id)
        {
            return await _context.Class.OfType<OnlineClass>().Where(c => c.Id == id).Include(c => c.Category).Include(c => c.Instructor).Include(c => c.Clients).FirstOrDefaultAsync();
        }

        public async Task<List<VideoClass>> GetAllVideoClasses()
        {
            return await _context.Class.OfType<VideoClass>().Include(c => c.Category).ToListAsync();
        }

        public async Task<VideoClass> GetVideoClassByIdInclude(int id)
        {
            return await _context.Class.OfType<VideoClass>().Where(c => c.Id == id).Include(c => c.Category).FirstOrDefaultAsync();
        }

        public async Task<List<GymClass>> GetAllGymClassesInclude()
        {
            return await _context.Class.OfType<GymClass>().Include(c => c.Category).Include(c => c.Instructor).Include(c => c.Gym).Include(c => c.Clients).ToListAsync();
        }

        public async Task<GymClass> GetGymClassByIdInclude(int id)
        {
            return await _context.Class.OfType<GymClass>().Where(c => c.Id == id).Include(c => c.Category).Include(c => c.Instructor).Include(c => c.Clients).Include(c => c.Gym).FirstOrDefaultAsync();
        }

    }
}
