using FitnessHub.Data.Entities.GymMachines;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessHub.Data.Repositories
{
    public interface IMachineRepository : IGenericRepository<Machine>
    {
        Task<Machine> GetMachineByIdInclude(int id);

        Task<List<SelectListItem>> GetAllMachinesSelectList();

        Task<List<SelectListItem>> GetAllMachinesSelectListWithoutNoMachine();
    }
}
