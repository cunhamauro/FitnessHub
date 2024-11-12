using Microsoft.EntityFrameworkCore;
using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.Users;

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

        public async Task<Gym?> GetGymByUserAsync(User user)
        {
            if (user is Admin admin)
            {
                return await _context.Gyms.FirstOrDefaultAsync(g => g.Id == admin.GymId);
            }

            if (user is Employee employee)
            {
                return await _context.Gyms.FirstOrDefaultAsync(g => g.Id == employee.GymId);
            }

            if (user is Instructor instructor)
            {
                return await _context.Gyms.FirstOrDefaultAsync(g => g.Id == instructor.GymId);
            }

            if (user is Client client)
            {
                return await _context.Gyms.FirstOrDefaultAsync(g => g.Id == client.GymId);
            }

            return null;
        }

        public async Task<int> GetCountriesCountAsync()
        {
            return await _context.Gyms.Select(g => g.Country).Distinct().CountAsync();
        }
    }
}
