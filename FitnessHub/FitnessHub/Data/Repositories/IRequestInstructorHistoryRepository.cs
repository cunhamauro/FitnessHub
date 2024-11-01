using FitnessHub.Data.Entities.Communication;

namespace FitnessHub.Data.Repositories
{
    public interface IRequestInstructorHistoryRepository : IGenericRepository<RequestInstructorHistory>
    {
        List<RequestInstructorHistory> GetAllByGymId(int gymId);
    }
}
