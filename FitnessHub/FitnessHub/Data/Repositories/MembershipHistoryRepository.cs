using FitnessHub.Data.Entities.History;

namespace FitnessHub.Data.Repositories
{
    public class MembershipHistoryRepository : GenericRepository<MembershipHistory>, IMembershipHistoryRepository
    {
        public MembershipHistoryRepository(DataContext context) : base(context)
        {
        }
    }
}
