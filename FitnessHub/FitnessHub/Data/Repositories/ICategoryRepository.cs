using FitnessHub.Data.Entities.GymMachines;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessHub.Data.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<IEnumerable<SelectListItem>> GetCategoriesSelectListAsync();
    }
}
