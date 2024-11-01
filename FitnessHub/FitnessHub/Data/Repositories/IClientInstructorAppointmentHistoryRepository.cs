using FitnessHub.Data.Entities.History;

namespace FitnessHub.Data.Repositories
{
    public interface IClientInstructorAppointmentHistoryRepository : IGenericRepository<ClientInstructorAppointmentHistory>
    {
        List<ClientInstructorAppointmentHistory> GetAllByGymId(int gymId);
    }
}
