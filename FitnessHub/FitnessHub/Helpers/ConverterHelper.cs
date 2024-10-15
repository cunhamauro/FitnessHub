using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Repositories;
using FitnessHub.Models;

namespace FitnessHub.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        private readonly IMachineCategoryRepository _machineCategoryRepository;

        public ConverterHelper(IMachineCategoryRepository machineCategoryRepository)
        {
            _machineCategoryRepository = machineCategoryRepository;
        }
        public async Task<Machine> ToMachineAsync(MachineViewModel model, string? path, bool isNew)
        {
            return new Machine
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                TutorialVideoUrl = model.TutorialVideoUrl,
                ImagePath = path,
                Category = await _machineCategoryRepository.GetByIdTrackAsync(model.CategoryId),
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
                CategoryId = machine.Category.Id,
            };
        }

        public string YoutubeUrlToEmbed(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;

            if (url.Contains("youtu.be/"))
            {
                // Extract video ID from shortened URL
                var videoId = url.Split("youtu.be/")[1].Split("?")[0];
                return $"https://www.youtube.com/embed/{videoId}";
            }
            else if (url.Contains("youtube.com/watch?v="))
            {
                // Extract video ID from standard URL
                var videoId = url.Split("v=")[1].Split("&")[0];
                return $"https://www.youtube.com/embed/{videoId}";
            }
            else if (url.Contains("youtube.com/embed/"))
            {
                // Already in embed format
                return url;
            }

            return string.Empty; // Return an empty string if URL format is not recognized
        }
    }
}
