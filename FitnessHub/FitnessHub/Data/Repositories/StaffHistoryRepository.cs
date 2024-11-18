using FitnessHub.Data.Entities.History;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class StaffHistoryRepository : IStaffHistoryRepository
    {
        private readonly DataContext _context;

        public StaffHistoryRepository(DataContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(StaffHistory staffHistory)
        {
            await _context.Set<StaffHistory>().AddAsync(staffHistory);
            await SaveAllAsync();
        }

        public IQueryable<StaffHistory> GetAll()
        {
            return _context.Set<StaffHistory>().AsNoTracking();
        }

        public IQueryable<StaffHistory> GetByGymId(int gymId)
        {
            return _context.Set<StaffHistory>().Where(c => c.GymId == gymId).AsNoTracking();
        }

        public async Task<StaffHistory?> GetByStaffIdAndGymIdTrackAsync(string staffId, int gymId)
        {
            return await _context.Set<StaffHistory>().FirstOrDefaultAsync(e => e.StaffId == staffId && e.GymId == gymId);
        }

        public async Task UpdateAsync(StaffHistory staffHistory)
        {
            _context.Set<StaffHistory>().Update(staffHistory);
            await SaveAllAsync();
        }

        private async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
