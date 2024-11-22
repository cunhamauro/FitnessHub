using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.History;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class ClientHistoryRepository : IClientHistoryRepository
    {
        private readonly DataContext _context;

        public ClientHistoryRepository(DataContext context)
        {
            _context = context;
        }

        public IQueryable<ClientHistory> GetAll()
        {
            return _context.Set<ClientHistory>().AsNoTracking();
        }

        public IQueryable<ClientHistory> GetByGymId(int gymId)
        {
            return _context.Set<ClientHistory>().Where(c => c.GymId == gymId).AsNoTracking();
        }

        public async Task CreateAsync(ClientHistory clientHistory)
        {
            await _context.Set<ClientHistory>().AddAsync(clientHistory);
            await SaveAllAsync();
        }

        public async Task UpdateAsync(ClientHistory clientHistory)
        {
            _context.Set<ClientHistory>().Update(clientHistory);
            await SaveAllAsync();
        }

        public async Task<ClientHistory?> GetByIdTrackAsync(string id)
        {
            return await _context.Set<ClientHistory>().FirstOrDefaultAsync(e => e.Id == id);
        }

        private async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }


    }
}
