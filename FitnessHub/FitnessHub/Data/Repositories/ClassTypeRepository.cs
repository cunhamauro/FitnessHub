using FitnessHub.Data.Entities.GymClasses;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class ClassTypeRepository : GenericRepository<ClassType>, IClassTypeRepository
    {
        private readonly DataContext _context;

        public ClassTypeRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<SelectListItem>> GetTypesSelectListAsync()
        {
            var types = await _context.ClassTypes.ToListAsync();

            return types.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
        }

        public async Task<List<ClassType>> GetTypeFromCategory(int categoryId)
        {
            return await _context.ClassTypes.Include(t => t.ClassCategory).Where(t => t.ClassCategory.Id == categoryId).ToListAsync();
        }

        public async Task<ClassType?> GetByIdIncludeCategory(int id)
        {
            return await _context.ClassTypes.Include(c => c.ClassCategory).Where(c => c.Id == id).FirstOrDefaultAsync();
        }
    }
}
