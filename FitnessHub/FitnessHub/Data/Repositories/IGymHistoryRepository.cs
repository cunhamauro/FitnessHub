using FitnessHub.Data.Entities.History;

namespace FitnessHub.Data.Repositories
{
    public interface IGymHistoryRepository : IGenericRepository<GymHistory>
    {
        Task<GymHistory?> GetByName(string name);
    }
}
