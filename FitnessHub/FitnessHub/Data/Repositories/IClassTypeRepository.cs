using FitnessHub.Data.Entities.GymClasses;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessHub.Data.Repositories
{
    public interface IClassTypeRepository : IGenericRepository<ClassType>
    {
        Task<List<SelectListItem>> GetTypesSelectListAsync();

        Task<List<ClassType>> GetTypeFromCategory(int categoryId);

        Task<ClassType?> GetByIdIncludeCategory(int id);
    }
}
