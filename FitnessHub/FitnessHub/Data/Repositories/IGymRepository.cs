using FitnessHub.Data.Entities;

namespace FitnessHub.Data.Repositories
{
    public interface IGymRepository : IGenericRepository<Gym>
    {
        Task<List<string?>> GetCitiesByCountryAsync(string countryName);
    }
}
