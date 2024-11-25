using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.Communication;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class RequestInstructorRepository : GenericRepository<RequestInstructor>, IRequestInstructorRepository
    {
        private readonly DataContext _context;

        public RequestInstructorRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public List<RequestInstructor> GetAllByGymWithClients(int gymId)
        {
            return _context.RequestsIntructor.Where(requests => requests.GymId == gymId).Include(requests => requests.Client).ToList();
        }

        public async Task<RequestInstructor?> GetRequestByIdWithClient(int id)
        {
            return await _context.RequestsIntructor
                .Where(request => request.Id == id)
                .Include(requests => requests.Client)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ClientHasPendingRequestForGym(string clientId, int gymId)
        {
            var request = await _context.RequestsIntructor
                .Where(request => request.Client.Id == clientId && request.GymId == gymId)
                .FirstOrDefaultAsync();

            if(request != null)
                return true;
            return false;
        }
    }
}
