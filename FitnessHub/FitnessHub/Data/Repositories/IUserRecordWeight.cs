using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.GymClasses;

namespace FitnessHub.Data.Repositories
{
    public interface IUserRecordWeight : IGenericRepository<WeightProgress>
    {
        Task<List<WeightProgress>> GetWeightProgressAsync(string userId);

        Task<WeightProgress?> GetLastWeightProgress(string userId);
    }
}
