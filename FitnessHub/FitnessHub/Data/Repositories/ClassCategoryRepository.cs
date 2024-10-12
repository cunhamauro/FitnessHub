using FitnessHub.Data.Entities.GymClasses;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class ClassCategoryRepository : GenericRepository<ClassCategory>, IClassCategoryRepository
    {
        private readonly DataContext _context;
        public ClassCategoryRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<SelectListItem>> GetCategoriesSelectListAsync()
        {
            var categories = await _context.ClassCategories.ToListAsync();

            return categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
        }
    }
}
