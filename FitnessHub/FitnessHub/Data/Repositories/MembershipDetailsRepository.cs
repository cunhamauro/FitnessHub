using FitnessHub.Data.Entities.Users;
using FitnessHub.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FitnessHub.Data.Repositories
{
    public class MembershipDetailsRepository : GenericRepository<MembershipDetails>, IMembershipDetailsRepository
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public MembershipDetailsRepository(DataContext context, IUserHelper userHelper) : base(context)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task<MembershipDetails> GetMembershipDetailsByIdIncludeMembership(int id)
        {
            return await _context.MembershipDetails.Where(m => m.Id == id).Include(m => m.Membership).FirstOrDefaultAsync();
        }

        public async Task<bool> IsMemberShipInDetails(int id)
        {
            return await _context.MembershipDetails.AnyAsync(md => md.Membership.Id == id);
        }

        public async Task<decimal> GetAnualMembershipsRevenueAsync()
        {
            var membershipDetails = await _context.MembershipDetails.Include(m => m.Membership).ToListAsync();

            decimal anualRevenue = 0;

            foreach (var membership in membershipDetails)
            {
                var membershipAnualIncome = membership.Membership.MonthlyFee * 12;

                anualRevenue += membershipAnualIncome;
            }

            return anualRevenue;
        }

        

        public async Task<bool> ClientHasMemberShip(ClaimsPrincipal client)
        {
            var user = await _userHelper.GetUserAsync(client) as Client;

            if (user == null)
            {
                return false;
            }

            if (user.MembershipDetailsId == null )
            {
                return false;
            }

            var memberShipDetailClient =  await _context.MembershipDetails.Where(m => m.Id == user.MembershipDetailsId).FirstOrDefaultAsync(); ;

            if (user.MembershipDetailsId == null || memberShipDetailClient == null)
            {
                return false;
            }

            if (memberShipDetailClient.Status == false)
            {
                return false;
            }

            return true;
        }
    }
}
