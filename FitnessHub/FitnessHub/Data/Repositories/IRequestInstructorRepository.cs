using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.Communication;

namespace FitnessHub.Data.Repositories
{
    public interface IRequestInstructorRepository : IGenericRepository<RequestInstructor>
    {
        List<RequestInstructor> GetAllByGymWithClients(Gym gym);

        Task<RequestInstructor?> GetByIdWithClientAndGym(int id);
    }
}
