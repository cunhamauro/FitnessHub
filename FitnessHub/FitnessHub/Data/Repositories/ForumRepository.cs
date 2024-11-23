using FitnessHub.Data.Entities.Communication;
using FitnessHub.Data.Entities.GymClasses;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class ForumRepository : GenericRepository<ForumThread>, IForumRepository
    {
        private readonly DataContext _context;

        public ForumRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ForumThread> GetForumPostByIdTrackIncludeAsync(int id)
        {
            return await _context.ForumThreads.Where(f => f.Id == id).Include(f => f.User).Include(f => f.ForumReplies).ThenInclude(m => m.User).FirstOrDefaultAsync();            
        }
    }
}
