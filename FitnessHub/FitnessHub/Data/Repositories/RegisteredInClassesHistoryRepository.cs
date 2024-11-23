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

            // Associar cada registro em ClassesRegistrationHistory ao seu ClassType no ClassHistory
            var classHistories = await _context.ClassHistory
                .Where(ch => classRegistrations.Select(cr => cr.ClassId).Contains(ch.Id))
                .ToListAsync();

            // Criar uma lista que reflita cada ocorrência no ClassesRegistrationHistory
            var classTypes = classRegistrations
                .Join(classHistories,
                    cr => cr.ClassId,  // Chave em ClassesRegistrationHistory
                    ch => ch.Id,       // Chave em ClassHistory
                    (cr, ch) => ch.ClassType) // Selecionar ClassType correspondente
                .ToList();

            // Agrupar por ClassType e contar as ocorrências
            var groupedClasses = classTypes
                .GroupBy(ct => ct)
                .Select(group => new
                {
                    ClassType = group.Key,
                    Count = group.Count()
                })
                .ToList();

            // Determinar o maior número de repetições
            var maxCount = groupedClasses.Max(g => g.Count);

            // Obter o(s) tipo(s) de classe(s) mais popular(es)
            var mostPopularClassesType = groupedClasses
                .Where(g => g.Count == maxCount)
                .Select(g => g.ClassType)
                .ToList();

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
