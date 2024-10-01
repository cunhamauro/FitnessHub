using FitnessHub.Data.Entities.GymMachines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly DataContext _context;
        public CategoryRepository(DataContext context) : base(context)
        {      
            _context = context;
        }

        public async Task<IEnumerable<SelectListItem>> GetCategoriesSelectListAsync()
        {
            var categories = await GetAll().ToListAsync(); 

            return categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),  
                Text = c.Name             
            });
        }
    }
}
