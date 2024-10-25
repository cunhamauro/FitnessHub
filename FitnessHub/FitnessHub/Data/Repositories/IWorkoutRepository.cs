using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Entities.Users;

namespace FitnessHub.Data.Repositories
{
    public interface IWorkoutRepository : IGenericRepository<Workout>
    {
        Task<IEnumerable<Workout>> GetAllWorkoutsIncludeAsync();

        Task<Workout> GetWorkoutByIdIncludeAsync(int id);

        Task<List<Workout>> GetClientWorkoutsIncludeAsync(Client client);
    }
}
