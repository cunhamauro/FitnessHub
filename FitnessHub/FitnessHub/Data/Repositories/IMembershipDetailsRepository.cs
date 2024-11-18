using FitnessHub.Data.Entities.Users;
using System.Security.Claims;

namespace FitnessHub.Data.Repositories
{
    public interface IMembershipDetailsRepository : IGenericRepository<MembershipDetails>
    {
        Task<MembershipDetails> GetMembershipDetailsByIdIncludeMembership(int id);

        Task<bool> IsMemberShipInDetails(int id);
        
        Task<decimal> GetAnualMembershipsRevenueAsync();

        Task<bool> ClientHasMemberShip(ClaimsPrincipal client);

    }
}
