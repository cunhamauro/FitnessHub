using FitnessHub.Data.Entities.GymMachines;

namespace FitnessHub.Data.Repositories
{
    public class MachineDetailRepository : GenericRepository<MachineDetail>, IMachineDetailRepository
    {
        public MachineDetailRepository(DataContext context) : base(context)
        {
        }
    }
}
