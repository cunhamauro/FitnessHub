using FitnessHub.Data.Entities.GymClasses;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class RegisteredInClassesHistoryRepository : GenericRepository<RegisteredInClassesHistory>, IRegisteredInClassesHistoryRepository
    {
        private readonly DataContext _context;

        public RegisteredInClassesHistoryRepository(DataContext context) : base(context)
        {
           _context = context;
        }

        public async Task<RegisteredInClassesHistory?> GetHistoryEntryAsync(int classId, string userId)
        {
            return await _context.ClassesRegistrationHistory
                .FirstOrDefaultAsync(h => h.ClassId == classId && h.UserId == userId && !h.Canceled);
        }

    }
}
