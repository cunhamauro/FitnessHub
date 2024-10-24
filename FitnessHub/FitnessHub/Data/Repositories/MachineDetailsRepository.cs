using FitnessHub.Data.Entities.GymMachines;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class MachineDetailsRepository : GenericRepository<MachineDetails>, IMachineDetailsRepository
    {
        private readonly DataContext _context;

        public MachineDetailsRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<MachineDetails> GetAllByGymWithMachinesAndGyms(int gymId)
        {
            return _context.MachineDetails
                .Where(mc => mc.Gym.Id == gymId)
                .Include(mc => mc.Gym)
                .Include(mc => mc.Machine);
        }

        public Task<MachineDetails?> GetByIdWithMachines(int id)
        {
            return _context.MachineDetails
                .Where(mc => mc.Id == id)
                .Include(mc => mc.Machine)
                .FirstOrDefaultAsync();
        }
    }
}
