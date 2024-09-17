using FitnessHub.Data.Entities.GymClasses;

namespace FitnessHub.Data.Repositories
{
    public class ClassRepository : GenericRepository<Class>, IClassRepository
    {
        public ClassRepository(DataContext context) : base(context)
        {
        }
    }
}
