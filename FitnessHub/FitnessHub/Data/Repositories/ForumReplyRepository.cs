using FitnessHub.Data.Entities.Communication;
using FitnessHub.Data.Entities.GymClasses;

namespace FitnessHub.Data.Repositories
{
    public class ForumReplyRepository : GenericRepository<ForumReply>, IForumReplyRepository
    {
        public ForumReplyRepository(DataContext context) : base(context)
        {
        }
    }
}
