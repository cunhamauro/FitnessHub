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
                .Include(mc => mc.Machine)
                .ThenInclude(mc => mc.Category);
        }

        public async Task<MachineDetails?> GetByIdWithMachines(int id)
        {
            return await _context.MachineDetails
                .Where(mc => mc.Id == id)
                .Include(mc => mc.Machine)
                .ThenInclude(mc => mc.Category)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsMachineInDetails(int id)
        {
            return await _context.MachineDetails.AnyAsync(md => md.Machine.Id == id);
        }
    }
}
