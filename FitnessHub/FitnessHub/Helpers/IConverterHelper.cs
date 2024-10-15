using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Models;

namespace FitnessHub.Helpers
{
    public interface IConverterHelper
    {
        string YoutubeUrlToEmbed(string url);

        Task<Machine> ToMachineAsync(MachineViewModel model, string path, bool isNew);

        MachineViewModel ToMachineViewModel(Machine machine);
    }
}
