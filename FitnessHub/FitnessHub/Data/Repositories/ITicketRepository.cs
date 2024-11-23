﻿using FitnessHub.Data.Entities.Communication;

namespace FitnessHub.Data.Repositories
{
    public interface ITicketRepository : IGenericRepository<Ticket>
    {
        Task<List<Ticket>> GetTicketsByUserTrackIncludeAsync(string id);

        Task<Ticket> GetTicketByIdTrackIncludeAsync(int id);
    }
}