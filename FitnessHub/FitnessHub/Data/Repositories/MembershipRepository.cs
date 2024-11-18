using FitnessHub.Data.Entities.Users;

namespace FitnessHub.Data.Repositories
{
    public class MembershipRepository : GenericRepository<Membership>, IMembershipRepository
    {
        public MembershipRepository(DataContext context) : base(context)
        {

        }


    }
}
