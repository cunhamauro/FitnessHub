using FitnessHub.Data.Entities.Communication;

namespace FitnessHub.Data.Repositories
{
    public interface IClientInstructorAppointmentHistoryRepository : IGenericRepository<ClientInstructorAppointmentHistory>
    {
        List<ClientInstructorAppointmentHistory> GetAllByGymId(int gymId);
    }
}
