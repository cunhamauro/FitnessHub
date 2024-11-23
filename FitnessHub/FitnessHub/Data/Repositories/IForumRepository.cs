using FitnessHub.Data.Entities.Communication;
using FitnessHub.Data.Entities.GymClasses;

namespace FitnessHub.Data.Repositories
{
    public interface IForumRepository : IGenericRepository<ForumThread>
    {
        Task<ForumThread> GetForumPostByIdTrackIncludeAsync(int id);
    }
}
