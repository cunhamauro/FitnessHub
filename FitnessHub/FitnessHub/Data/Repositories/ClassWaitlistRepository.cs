using FitnessHub.Data.Entities.GymClasses;

namespace FitnessHub.Data.Repositories
{
    public class ClassWaitlistRepository : GenericRepository<ClassWaitlist>, IClassWaitlistRepository
    {
        public ClassWaitlistRepository(DataContext context) : base(context)
        {
        }
    }
}
