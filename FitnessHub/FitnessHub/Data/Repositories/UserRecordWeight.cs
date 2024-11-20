using FitnessHub.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class UserRecordWeight : GenericRepository<WeightProgress>, IUserRecordWeight
    {
        private readonly DataContext _context;

        public UserRecordWeight(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<WeightProgress>> GetWeightProgressAsync(string userId)
        {
            return await _context.WeightProgress
                                 .Where(w => w.CustomerId == userId)
                                 .OrderByDescending(w => w.Date)
                                 .ToListAsync();
        }

        public async Task<WeightProgress?> GetLastWeightProgress(string userId)
        {
            return await _context.WeightProgress
                                 .Where(w => w.CustomerId == userId)
                                 .OrderByDescending(w => w.Date)
                                 .FirstOrDefaultAsync();
        }
    }
}
