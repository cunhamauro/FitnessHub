using FitnessHub.Data.Entities.History;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class GymHistoryRepository : GenericRepository<GymHistory>, IGymHistoryRepository
    {
        private readonly DataContext _context;

        public GymHistoryRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<GymHistory?> GetByName(string name)
        {
            return await _context.GymsHistory.Where(g => g.Name == name).FirstOrDefaultAsync();
        }
    }
}
