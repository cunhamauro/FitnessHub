using FitnessHub.Data.Entities.GymMachines;

namespace FitnessHub.Data.Repositories
{
    public interface IExerciseRepository : IGenericRepository<Exercise>
    {
        Task<bool> IsMachineIsInExercice(int id);
    }
}
