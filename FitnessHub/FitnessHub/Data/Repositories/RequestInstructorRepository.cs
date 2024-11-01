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

        public List<RequestInstructor> GetAllByGymWithClients(Gym gym)
        {
            return _context.RequestsIntructor.Where(requests => requests.Gym == gym).Include(requests => requests.Client).ToList();
        }

        public async Task<RequestInstructor?> GetByIdWithClientAndGym(int id)
        {
            return await _context.RequestsIntructor
                .Where(request => request.Id == id)
                .Include(requests => requests.Client)
                .Include(requests => requests.Gym)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ClientHasPendingRequestForGym(string clientId, int gymId)
        {
            var request = await _context.RequestsIntructor
                .Where(request => request.Client.Id == clientId && request.Gym.Id == gymId)
                .FirstOrDefaultAsync();

            if(request != null)
                return true;
            return false;
        }
    }
}
