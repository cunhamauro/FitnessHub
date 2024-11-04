using FitnessHub.Data.Entities.History;

namespace FitnessHub.Data.Repositories
{
    public class GymHistoryRepository : GenericRepository<GymHistory>, IGymHistoryRepository
    {
        public GymHistoryRepository(DataContext context) : base(context)
        {
        }
    }
}
