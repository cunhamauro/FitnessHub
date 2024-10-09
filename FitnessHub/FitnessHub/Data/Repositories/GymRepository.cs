using FitnessHub.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Data.Repositories
{
    public class GymRepository : GenericRepository<Gym>, IGymRepository
    {
        private readonly DataContext _context;

        public GymRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<string?>> GetCitiesByCountryAsync(string countryName)
        {
            return await _context.Gyms
                .Where(g => g.Country == countryName)
                .Select(g => g.City)
                .Distinct()
                .ToListAsync();
        }
    }
}
