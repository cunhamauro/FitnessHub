using FitnessHub.Data.Entities.Communication;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class ClientInstructorAppointmentRepository : GenericRepository<ClientInstructorAppointment>, IClientInstructorAppointmentRepository
    {
        private readonly DataContext _context;

        public ClientInstructorAppointmentRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public List<ClientInstructorAppointment> GetAllByInstructorWithClients(string id)
        {
            return _context.ClientInstructorAppointments.Where(a => a.InstructorId == id).Include(a => a.Client).ToList();
        }
    }
}
