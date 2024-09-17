using FitnessHub.Data.Entities.GymMachines;

namespace FitnessHub.Data.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {        
        public CategoryRepository(DataContext context) : base(context)
        {            
        }
    }
}
