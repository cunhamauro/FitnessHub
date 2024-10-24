using FitnessHub.Data.Entities.GymMachines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class MachineRepository : GenericRepository<Machine>, IMachineRepository
    {
        private readonly DataContext _context;

        public MachineRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Machine> GetMachineByIdInclude(int id)
        {
            return await _context.Machines.Where(m => m.Id == id).Include(m => m.Category).FirstOrDefaultAsync();
        }

        public async Task<List<SelectListItem>> GetAllMachinesSelectList()
        {
            var machines = await _context.Machines.ToListAsync();

            var machineSelectList = machines.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();

            return machineSelectList;
        }

        public async Task<List<SelectListItem>> GetAllMachinesSelectListWithoutNoMachine()
        {
            var machines = await _context.Machines.ToListAsync();

            Machine? noMachine = _context.Machines.Where(m => m.Name == "No Machine").FirstOrDefault();

            var machinesWithoutNoMachine = machines.Remove(noMachine);

            var machineSelectList = machines.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();

            return machineSelectList;
        }
    }

}
