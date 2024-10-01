using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Controllers
{
    public class MachinesController : Controller
    {
        private readonly IMachineRepository _machineRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly IImageHelper _imageHelper;

        public MachinesController(IMachineRepository machineRepository,
            ICategoryRepository categoryRepository,
            IConverterHelper converterHelper,
            IImageHelper imageHelper)
        {
            _machineRepository = machineRepository;
            _categoryRepository = categoryRepository;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
        }

        // GET: Machines
        public async Task<IActionResult> Index()
        {
            return View(await _machineRepository.GetAll().ToListAsync());
        }

        // GET: Machines/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _machineRepository.GetAll()
           .Include(m => m.Category) 
           .FirstOrDefaultAsync(m => m.Id == id);

            if (machine == null)
            {
                return NotFound();
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
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _machineRepository.GetByIdAsync(id.Value);

            if (machine == null)
            {
                return NotFound();
            }

            var model = _converterHelper.ToMachineViewModel(machine);

            model.Categories = await _categoryRepository.GetCategoriesSelectListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MachineViewModel model)
        {
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
                        return NotFound();
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
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _machineRepository.GetByIdAsync(id.Value);

            if (machine == null)
            {
                return NotFound();
            }

            return View(machine);
        }

        // POST: Machines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var machine = await _machineRepository.GetByIdAsync(id);

            if (machine != null)
            {
                await _machineRepository.DeleteAsync(machine);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}