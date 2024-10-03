using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Repositories;
using FitnessHub.Models;

namespace FitnessHub.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        private readonly ICategoryRepository _categoryRepository;

        public ConverterHelper(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<Machine> ToMachineAsync(MachineViewModel model, string? path, bool isNew)
        {
            return new Machine
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                TutorialVideoUrl = model.TutorialVideoUrl,
                ImagePath = path,
                Category = await _categoryRepository.GetByIdTrackAsync(model.CategoryId),
            };
        }

        public MachineViewModel ToMachineViewModel(Machine machine)
        {
            return new MachineViewModel
            {
                Id = machine.Id,
                Name = machine.Name,
                TutorialVideoUrl = machine.TutorialVideoUrl,
                ImagePath = machine.ImagePath,
                Category = machine.Category,
                CategoryId = machine.Category?.Id ?? 0 
            };
        }
    }
}
