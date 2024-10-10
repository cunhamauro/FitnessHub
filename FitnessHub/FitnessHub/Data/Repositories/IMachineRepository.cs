using FitnessHub.Data.Entities.GymMachines;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessHub.Data.Repositories
{
    public interface IMachineRepository : IGenericRepository<Machine>
    {
        Task<List<SelectListItem>> GetAllMachinesAsync();
    }
}
