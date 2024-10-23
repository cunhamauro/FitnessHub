﻿using FitnessHub.Data.Entities.Users;

namespace FitnessHub.Data.Repositories
{
    public interface IMembershipDetailsRepository : IGenericRepository<MembershipDetails>
    {
        Task<MembershipDetails> GetMembershipDetailsByIdIncludeMembership(int id);
    }
}
