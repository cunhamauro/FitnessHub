using FitnessHub.Data.Entities.GymClasses;

namespace FitnessHub.Data.Repositories
{
    public interface IClassRepository : IGenericRepository<Class>
    {
        Task<List<OnlineClass>> GetAllOnlineClassesInclude();

        Task<List<VideoClass>> GetAllVideoClassesInclude();

        Task<List<GymClass>> GetAllGymClassesInclude();

        Task<GymClass> GetGymClassByIdInclude(int id);

        Task<OnlineClass> GetOnlineClassByIdInclude(int id);

        Task<VideoClass> GetVideoClassByIdInclude(int id);

        Task<GymClass?> GetGymClassByIdIncludeTracked(int id);

        Task<OnlineClass?> GetOnlineClassByIdIncludeTracked(int id);

        Task<List<GymClass>> GetClassesByGym(int gymId);
    }
}
