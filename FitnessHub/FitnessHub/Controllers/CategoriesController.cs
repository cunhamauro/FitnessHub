using System;
using System.Collections.Generic;
using System.Linq;
using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Repositories;
using FitnessHub.Models;
using Microsoft.AspNetCore.Mvc;

namespace FitnessHub.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IMachineCategoryRepository _categoryRepository;

        public CategoriesController(IMachineCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        // GET: CategoriesController
        public IActionResult Index()
        {
            var categories = _categoryRepository.GetAll().OrderBy(e => e.Name);
            return View(categories);
        }

        // GET: CategoriesController/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return CategoryNotFound();
            }

            var category = await _categoryRepository.GetByIdAsync(id.Value);

            if (category == null)
            {
                return CategoryNotFound();
            }
            return View(category);
        }

        // GET: CategoriesController/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CategoriesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MachineCategory category)
        {
            List<MachineCategory> categories = _categoryRepository.GetAll().ToList();

            foreach (var cat in categories)
            {
                if (cat.Name == category.Name)
                {
                    ModelState.AddModelError("Name", "There is already a category with this name");
                }
            }

            if (ModelState.IsValid)
            {
                await _categoryRepository.CreateAsync(category);
                return RedirectToAction("Index");
            }
            return View();
        }

        // GET: CategoriesController/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return CategoryNotFound();
            }

            var category = await _categoryRepository.GetByIdAsync(id.Value);

            if (category == null)
            {
                return CategoryNotFound();
            }
            return View(category);
        }

        // POST: CategoriesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MachineCategory category)
        {
            MachineCategory oldCategory = await _categoryRepository.GetByIdAsync(category.Id);
            List<MachineCategory> categories = _categoryRepository.GetAll().ToList();

            foreach (var cat in categories)
            {
                if (cat.Name == category.Name && cat.Name != oldCategory.Name)
                {
                    ModelState.AddModelError("Name", "There is already a category with this name");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryRepository.UpdateAsync(category);
                }
                catch (Exception)
                {
                    if (!await _categoryRepository.ExistsAsync(category.Id))
                    {
                        return CategoryNotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View();

        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return CategoryNotFound();
            }

            var category = await _categoryRepository.GetByIdAsync(id.Value);

            if (category == null)
            {
                return CategoryNotFound();
            }

            return View(category);
        }

        // POST: CategoriesController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                return CategoryNotFound();
            }

            await _categoryRepository.DeleteAsync(category);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult CategoryNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Category not found", Message = "Maybe it got lost at the gym?" });
        }

        public IActionResult DisplayMessage(string title, string message)
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = $"{title}", Message = $"{message}" });
        }
    }
}
