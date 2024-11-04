using FitnessHub.Data.Entities.History;

namespace FitnessHub.Data.Repositories
{
    public class ClassHistoryRepository : GenericRepository<ClassHistory>, IClassHistoryRepository
    {
        private readonly DataContext _context;

        public ClassHistoryRepository(DataContext context) : base(context)
        {
            _context = context;
        }
    }
}
