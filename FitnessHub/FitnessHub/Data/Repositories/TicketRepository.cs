using FitnessHub.Data.Entities.Communication;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        private readonly DataContext _context;

        public TicketRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Ticket> GetTicketByIdTrackIncludeAsync(int id)
        {
            return await _context.Tickets.Where(f => f.Id == id).Include(f => f.Client).Include(f => f.Staff).Include(f => f.TicketMessages).FirstOrDefaultAsync();
        }

        public async Task<List<Ticket>> GetTicketsByUserTrackIncludeAsync(string id)
        {
            return await _context.Tickets.Include(f => f.Client).Include(f => f.Staff).Include(f => f.TicketMessages).Where(f => id == f.Client.Id).ToListAsync();
        }

        public async Task<Ticket> GetTicketByStaffAndClientReplyTrueTrackIncludeAsync(string id)
        {
            return await _context.Tickets
                .Where(f => f.Staff.Id == id && f.HasClientReply == true)
                .Include(f => f.Client)
                .Include(f => f.Staff)
                .Include(f => f.TicketMessages)
                .FirstOrDefaultAsync();
        }

        public async Task<Ticket> GetTicketsByClientAndStaffReplyTrueTrackIncludeAsync(string id)
        {
            return await _context.Tickets
                .Where(f => id == f.Client.Id && f.Open == true && f.HasStaffReply == true)
                .Include(f => f.Client)
                .Include(f => f.Staff)
                .Include(f => f.TicketMessages)
                .FirstOrDefaultAsync();
        }
    }
}
