using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Repositories;
using FitnessHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Controllers
{
    public class MachinesController : Controller
    {
        private readonly IMachineRepository _machineRepository;
        private readonly ICategoryRepository _categoryRepository;

        public MachinesController(IMachineRepository machineRepository,
            ICategoryRepository categoryRepository)
        {
            _machineRepository = machineRepository;
            _categoryRepository = categoryRepository;
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

            var machine = await _machineRepository.GetByIdAsync(id.Value);
             
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
                var machine = new Machine
                {
                    Name = model.Name,
                    CategoryId = model.CategoryId,
                    TutorialVideoUrl = model.TutorialVideoUrl,
                    
                };

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
            return View(machine);
        }

        // POST: Machines/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Machine machine)
        {
            if (id != machine.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _machineRepository.UpdateAsync(machine);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _machineRepository.ExistsAsync(machine.Id))
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
            return View(machine);
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
