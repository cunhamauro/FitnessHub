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

        public async Task<List<SelectListItem>> GetAllMachinesAsync()
        {
            var machines = await _context.Machines.ToListAsync();

            var machineSelectList = machines.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),  
                Text = m.Name             
            }).ToList();

            return machineSelectList;
        }
    }
  
}
