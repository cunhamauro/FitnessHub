using FitnessHub.Data.Entities.GymMachines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class MachineCategoryRepository : GenericRepository<MachineCategory>, IMachineCategoryRepository
    {
        private readonly DataContext _context;
        public MachineCategoryRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SelectListItem>> GetCategoriesSelectListAsync()
        {
            var categories = await _context.MachineCategories.ToListAsync();

            return categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(), 
                Text = c.Name            
            });
        }
    }
}
