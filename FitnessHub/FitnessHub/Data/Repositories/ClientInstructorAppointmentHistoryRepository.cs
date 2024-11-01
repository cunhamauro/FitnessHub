using FitnessHub.Data.Entities.History;

namespace FitnessHub.Data.Repositories
{
    public class ClientInstructorAppointmentHistoryRepository : GenericRepository<ClientInstructorAppointmentHistory>, IClientInstructorAppointmentHistoryRepository
    {
        private readonly DataContext _context;

        public ClientInstructorAppointmentHistoryRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public List<ClientInstructorAppointmentHistory> GetAllByGymId(int gymId)
        {
            return _context.ClientInstructorAppointmentsHistory.Where(a => a.GymId == gymId).ToList();
        }
    }
}
