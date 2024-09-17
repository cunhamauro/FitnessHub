using SQLitePCL;

namespace FitnessHub.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public SeedDb(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
    }
}
