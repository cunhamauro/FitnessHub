using FitnessHub.Data.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class MembershipDetailsRepository : GenericRepository<MembershipDetails>, IMembershipDetailsRepository
    {
        private readonly DataContext _context;

        public MembershipDetailsRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<MembershipDetails> GetMembershipDetailsByIdIncludeMembership(int id)
        {
            return await _context.MembershipDetails.Where(m => m.Id == id).Include(m => m.Membership).FirstOrDefaultAsync();
        }

        public async Task<decimal> GetAnualMembershipsRevenueAsync()
        {
            var membershipDetails = await _context.MembershipDetails.Include(m => m.Membership).ToListAsync();

            decimal anualRevenue = 0;

            foreach (var membership in membershipDetails)
            {
                var membershipAnualIncome = membership.Membership.Price * 12;

                anualRevenue += membershipAnualIncome;
            }

            return anualRevenue;
        }
    }
}
