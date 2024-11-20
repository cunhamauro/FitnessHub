using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.History;

namespace FitnessHub.Data.Repositories
{
    public interface IClientHistoryRepository 
    {
        IQueryable<ClientHistory> GetAll();

        IQueryable<ClientHistory> GetByGymId(int gymId);

        Task CreateAsync(ClientHistory clientHistory);

        Task UpdateAsync(ClientHistory clientHistory);

        Task<ClientHistory?> GetByIdTrackAsync(string id);

        
    }
}
