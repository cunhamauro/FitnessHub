using FitnessHub.Data.Entities.GymClasses;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessHub.Data.Repositories
{
    public interface IClassCategoryRepository : IGenericRepository<ClassCategory>
    {
        Task<List<SelectListItem>> GetCategoriesSelectListAsync();
    }
}
