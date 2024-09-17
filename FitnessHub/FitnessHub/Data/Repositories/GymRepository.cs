using FitnessHub.Data.Entities;

namespace FitnessHub.Data.Repositories
{
    public class GymRepository : GenericRepository<Gym>, IGymRepository
    {
        public GymRepository(DataContext context) : base(context)
        {
        }
    }
}
