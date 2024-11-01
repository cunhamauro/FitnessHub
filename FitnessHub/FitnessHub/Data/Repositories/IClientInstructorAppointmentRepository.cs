using FitnessHub.Data.Entities.Communication;

namespace FitnessHub.Data.Repositories
{
    public interface IClientInstructorAppointmentRepository : IGenericRepository<ClientInstructorAppointment>
    {
        List<ClientInstructorAppointment> GetAllByInstructorWithClients(string id);
    }
}
