using FitnessHub.Data.Entities.History;

namespace FitnessHub.Data.Repositories
{
    public interface IRequestInstructorHistoryRepository : IGenericRepository<RequestInstructorHistory>
    {
        List<RequestInstructorHistory> GetAllByGymId(int gymId);

        List<RequestInstructorHistory> GetAllByClient(string clientId);
    }
}
