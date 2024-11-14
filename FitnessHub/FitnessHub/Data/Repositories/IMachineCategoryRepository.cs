using FitnessHub.Data.Entities.GymMachines;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessHub.Data.Repositories
{
    public interface IMachineCategoryRepository : IGenericRepository<MachineCategory>
    {
        Task<IEnumerable<SelectListItem>> GetCategoriesSelectListAsync();

        
    }
}
