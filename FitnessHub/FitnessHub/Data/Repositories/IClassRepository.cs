using FitnessHub.Data.Entities.GymClasses;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public interface IClassRepository : IGenericRepository<Class>
    {
        Task<List<OnlineClass>> GetAllOnlineClassesInclude();

        Task<List<VideoClass>> GetAllVideoClasses();

        Task<List<GymClass>> GetAllGymClassesInclude();

        Task<GymClass> GetGymClassByIdInclude(int id);

        Task<OnlineClass> GetOnlineClassByIdInclude(int id);

        Task<VideoClass> GetVideoClassByIdInclude(int id);

        Task<GymClass?> GetGymClassByIdIncludeTracked(int id);
    }
}
