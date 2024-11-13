using FitnessHub.Data.Entities.GymClasses;

namespace FitnessHub.Data.Repositories
{
    public interface IRegisteredInClassesHistoryRepository : IGenericRepository<RegisteredInClassesHistory>
    {
        Task<RegisteredInClassesHistory?> GetHistoryEntryAsync(int classId, string userId);

        Task<string> GetMostPopularClass();
    }
}
