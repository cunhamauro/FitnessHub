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

        public async Task<string> GetMostPopularClass()
        {
            var classRegistrations = await _context.ClassesRegistrationHistory.ToListAsync();

            if (!classRegistrations.Any())
            {
                return "N/A";
            }

            var classType = classRegistrations.GroupBy(c => c.ClassId)
                .Select(group => new
                {
                    ClassId = group.Key,
                    ClassCount = group.Count()
                })
                .ToList();

            var maxClassTypeCount = classType.Max(g => g.ClassCount);

            var mostPopularClassIds = classType
                                 .Where(g => g.ClassCount == maxClassTypeCount)
                                 .Select(g => g.ClassId)
                                 .ToList();

            var mostPopularClassesType = await _context.ClassHistory
                .Where(c => mostPopularClassIds.Contains(c.Id))
                .Select(c => c.ClassType)
                .ToListAsync();

            if (mostPopularClassesType.Any())
            {
                return $"{string.Join(", ", mostPopularClassesType)}";
            }
            else
            {
                return "N/A";
            }
        }
    }
}
