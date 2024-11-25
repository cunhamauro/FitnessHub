using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.Communication;

namespace FitnessHub.Data.Repositories
{
    public interface IRequestInstructorRepository : IGenericRepository<RequestInstructor>
    {
        List<RequestInstructor> GetAllByGymWithClients(int gymId);

        Task<RequestInstructor?> GetRequestByIdWithClient(int id);

        Task<bool> ClientHasPendingRequestForGym(string clientId, int gymId);
    }
}
