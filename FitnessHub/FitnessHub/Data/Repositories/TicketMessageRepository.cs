using FitnessHub.Data.Entities.Communication;
using FitnessHub.Data.Entities.GymClasses;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class TicketMessageRepository : GenericRepository<TicketMessage>, ITicketMessageRepository
    {
        public TicketMessageRepository(DataContext context) : base(context)
        {
        }
    }
}
