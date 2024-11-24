using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Controllers
{
    [Authorize(Roles = "MasterAdmin")]
    public class MachinesController : Controller
    {
        private readonly IMachineRepository _machineRepository;
        private readonly IMachineCategoryRepository _categoryRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IMachineDetailsRepository _machineDetailsRepository;
        private readonly IExerciseRepository _exerciseRepository;

        public MachinesController(IMachineRepository machineRepository,
            IMachineCategoryRepository categoryRepository,
            IConverterHelper converterHelper,
            IImageHelper imageHelper, 
            IMachineDetailsRepository machineDetailsRepository,
            IExerciseRepository exerciseRepository)
        {
            _machineRepository = machineRepository;
            _categoryRepository = categoryRepository;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
            _machineDetailsRepository = machineDetailsRepository;
            _exerciseRepository = exerciseRepository;
        }

        // GET: Machines
        public async Task<IActionResult> Index()
        {
            var machines = await _machineRepository.GetAll().ToListAsync();
            var noMachine = machines.Where(m => m.Name == "No Machine").FirstOrDefault();

            machines.Remove(noMachine);

            return View(machines);
        }

        // GET: Machines/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id == 1)
            {
                return MachineNotFound();
            }

            var machine = await _machineRepository.GetAll()
           .Include(m => m.Category)
           .FirstOrDefaultAsync(m => m.Id == id);

            if (machine == null)
            {
                return MachineNotFound();
            }

            return View(machine);
        }

        // GET: Machines/Create
        public async Task<IActionResult> Create()
        {
            var model = new MachineViewModel
            {
                Categories = await _categoryRepository.GetCategoriesSelectListAsync()
            };
            return View(model);
        }

        // POST: Machines/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MachineViewModel model)
        {
            if (model.CategoryId < 1)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid Category");
            }

            if (model.TutorialVideoUrl != null && model.TutorialVideoUrl.Length > 1 && !model.TutorialVideoUrl.ToLower().Contains("youtu"))
            {
                ModelState.AddModelError("TutorialVideoUrl", "Only URL's from Youtube videos are accepted");
            }

            if (await _machineRepository.GetAll().AnyAsync(m => m.Name == model.Name))
            {
                ModelState.AddModelError("Name", "There already exists a Machine with this name");
            }

            if (ModelState.IsValid)
            {
                var path = string.Empty;

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    path = await _imageHelper.UploadImageAsync(model.ImageFile, "machines");
                }

                var machine = await _converterHelper.ToMachineAsync(model, path, true);

                await _machineRepository.CreateAsync(machine);

                return RedirectToAction(nameof(Index));
            }
            model.Categories = await _categoryRepository.GetCategoriesSelectListAsync();
            return View(model);
        }

        // GET: Machines/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 1)
            {
                return MachineNotFound();
            }

            var machine = await _machineRepository.GetMachineByIdInclude(id.Value);

            if (machine == null)
            {
                return MachineNotFound();
            }

            var model = _converterHelper.ToMachineViewModel(machine);

            model.Categories = await _categoryRepository.GetCategoriesSelectListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MachineViewModel model)
        {
            if (model.CategoryId < 1)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid Category");
            }

            if (model.TutorialVideoUrl != null && !model.TutorialVideoUrl.ToLower().Contains("youtu"))
            {
                ModelState.AddModelError("TutorialVideoUrl", "Only URL's from Youtube videos are accepted");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var path = model.ImagePath;

                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        path = await _imageHelper.UploadImageAsync(model.ImageFile, "machines");
                    }

                    var machine = await _converterHelper.ToMachineAsync(model, path, false);
                    await _machineRepository.UpdateAsync(machine);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _machineRepository.ExistsAsync(model.Id))
                    {
                        return MachineNotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Machines/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 1)
            {
                return MachineNotFound();
            }

            var machine = await _machineRepository.GetMachineByIdInclude(id.Value);

            if (machine == null)
            {
                return MachineNotFound();
            }

            return View(machine);
        }

        // POST: Machines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var machine = await _machineRepository.GetByIdAsync(id);

            if (machine == null)
            {
                return MachineNotFound();
            }

            var machineDetails = await _machineDetailsRepository.IsMachineInDetails(id);

            if (machineDetails)
            {
                ModelState.AddModelError(string.Empty, "This machine cannot be deleted because it is being used in a gym.");
                return View("Details", machine);
            }

            var machinesInExercices = await _exerciseRepository.IsMachineIsInExercice(id);

            if(machinesInExercices)
            {
                ModelState.AddModelError(string.Empty, "This machine cannot be deleted because it is being used in exercises.");
                return View("Details", machine);
            }

            await _machineRepository.DeleteAsync(machine);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult MachineNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Machine not found", Message = "Don't worry, there are plenty of other machines to get fit!" });
        }
    }
}