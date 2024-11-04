using FitnessHub.Data.Entities.History;

namespace FitnessHub.Data.Repositories
{
    public class ClientMembershipHistoryRepository : GenericRepository<ClientMembershipHistory>, IClientMembershipHistoryRepository
    {
        public ClientMembershipHistoryRepository(DataContext context) : base(context)
        {
        }
    }
}
