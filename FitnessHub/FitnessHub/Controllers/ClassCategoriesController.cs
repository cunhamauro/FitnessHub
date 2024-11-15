using FitnessHub.Data;
using FitnessHub.Data.Entities.GymClasses;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Controllers
{
    [Authorize(Roles = "MasterAdmin")]
    public class ClassCategoriesController : Controller
    {
        private readonly IClassCategoryRepository _classCategoryRepository;
        private readonly IImageHelper _imageHelper;
        private readonly IClassTypeRepository _classTypeRepository;

        public ClassCategoriesController(IClassCategoryRepository classCategoryRepository, IImageHelper imageHelper, IClassTypeRepository classTypeRepository)
        {
            _classCategoryRepository = classCategoryRepository;
            _imageHelper = imageHelper;
            _classTypeRepository = classTypeRepository;
        }

        // GET: ClassCategories
        public IActionResult Index()
        {
            return View(_classCategoryRepository.GetAll().OrderBy(c => c.Name));
        }

        // GET: ClassCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return CategoryNotFound();
            }

            var classCategory = await _classCategoryRepository.GetByIdAsync(id.Value);

            if (classCategory == null)
            {
                return CategoryNotFound();
            }

            return View(classCategory);
        }

        // GET: ClassCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassCategory category)
        {
          
            List<ClassCategory> categories = _classCategoryRepository.GetAll().ToList();

            foreach (var cat in categories)
            {
                if (cat.Name == category.Name)
                {
                    ModelState.AddModelError("Name", "There is already a class category with this name");
                }
            }

            if (string.IsNullOrEmpty(category.Description))
            {
                ModelState.AddModelError("Description", "Please enter a class category description");
            }

            if (ModelState.IsValid)
            {
                await _classCategoryRepository.CreateAsync(category);
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: ClassCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return CategoryNotFound();
            }

            var classCategory = await _classCategoryRepository.GetByIdAsync(id.Value);

            if (classCategory == null)
            {
                return CategoryNotFound();
            }

            return View(classCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClassCategory category)
        {
            ClassCategory oldCategory = await _classCategoryRepository.GetByIdAsync(category.Id);
            List<ClassCategory> categories = _classCategoryRepository.GetAll().ToList();

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
                    await _classCategoryRepository.UpdateAsync(category);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _classCategoryRepository.ExistsAsync(category.Id))
                    {
                        return CategoryNotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: ClassCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return CategoryNotFound();
            }

            var classCategory = await _classCategoryRepository.GetByIdAsync(id.Value);

            if (classCategory == null)
            {
                return CategoryNotFound();
            }

            return View(classCategory);
        }

        // POST: ClassCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var classCategory = await _classCategoryRepository.GetByIdAsync(id);

            if (classCategory == null)
            {
                return CategoryNotFound();
            }

            var classTypesWithThisCategory = await _classTypeRepository.GetAll().Where(ct => ct.ClassCategory.Id == classCategory.Id).ToListAsync();

            if(classTypesWithThisCategory.Any())
            {
                ModelState.AddModelError(string.Empty, "You cannot delete this category because it is associated with one or more class types");
                return View("Details", classCategory);
            }

            await _classCategoryRepository.DeleteAsync(classCategory);
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
