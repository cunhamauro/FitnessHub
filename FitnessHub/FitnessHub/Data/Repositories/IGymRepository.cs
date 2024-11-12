using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.Users;

namespace FitnessHub.Data.Repositories
{
    public interface IGymRepository : IGenericRepository<Gym>
    {
        Task<List<string?>> GetCitiesByCountryAsync(string countryName);

        Task<Gym?> GetGymByUserAsync(User user);

        Task<int> GetCountriesCountAsync();
    }
}
