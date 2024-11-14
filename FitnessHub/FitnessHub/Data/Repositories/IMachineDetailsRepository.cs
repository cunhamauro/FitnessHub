using FitnessHub.Data.Entities.GymMachines;

namespace FitnessHub.Data.Repositories
{
    public interface IMachineDetailsRepository : IGenericRepository<MachineDetails>
    {
        IQueryable<MachineDetails> GetAllByGymWithMachinesAndGyms(int gymId);

        Task<MachineDetails?> GetByIdWithMachines(int id);

        Task<bool> IsMachineInDetails(int id);
    }
}
