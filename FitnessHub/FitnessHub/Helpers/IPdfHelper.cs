using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Entities.Users;

namespace FitnessHub.Helpers
{
    public interface IPdfHelper
    {
        MemoryStream GenerateWorkoutPdf(Client client, Instructor instructor, Workout model);
    }
}
