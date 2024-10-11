using FitnessHub.Data.Entities.GymMachines;

namespace FitnessHub.Data.Repositories
{
    public interface IWorkoutRepository : IGenericRepository<Workout>
    {
        Task<IEnumerable<Workout>> GetAllWorkoutsInclude();

        Task<Workout> GetWorkoutByIdIncludeAsync(int id);
    }
}
