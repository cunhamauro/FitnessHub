using FitnessHub.Data.Entities.History;

namespace FitnessHub.Data.Repositories
{
    public interface IStaffHistoryRepository
    {
        IQueryable<StaffHistory> GetAll();

        IQueryable<StaffHistory> GetByGymId(int gymId);

        Task CreateAsync(StaffHistory staffHistory);

        Task UpdateAsync(StaffHistory staffHistory);

        Task<StaffHistory?> GetByStaffIdAndGymIdTrackAsync(string staffId, int gymId);
    }
}
