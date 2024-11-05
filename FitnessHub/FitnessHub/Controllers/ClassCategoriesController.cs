using FitnessHub.Data;
using FitnessHub.Data.Entities.GymClasses;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Controllers
{
    [Authorize(Roles = "MasterAdmin")]
    public class ClassCategoriesController : Controller
    {
        private readonly IClassCategoryRepository _classCategoryRepository;
        private readonly IImageHelper _imageHelper;

        public ClassCategoriesController(DataContext context, IClassCategoryRepository classCategoryRepository, IImageHelper imageHelper)
        {
            _classCategoryRepository = classCategoryRepository;
            _imageHelper = imageHelper;
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
                return NotFound();
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

        // POST: ClassCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(ClassCategory classCategory)
        //{
        //    List<ClassCategory> categories = _classCategoryRepository.GetAll().ToList();

        //    foreach (var cat in categories)
        //    {
        //        if (cat.Name == classCategory.Name)
        //        {
        //            ModelState.AddModelError("Name", "There is already a category with this name");
        //        }
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        await _classCategoryRepository.CreateAsync(classCategory);
        //        return RedirectToAction("Index");
        //    }
        //    return View(classCategory);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassCategoriesViewModel model)
        {
            List<ClassCategory> categories = _classCategoryRepository.GetAll().ToList();

            foreach (var cat in categories)
            {
                if (cat.Name == model.Name)
                {
                    ModelState.AddModelError("Name", "There is already a category with this name");
                }
            }

            if (ModelState.IsValid)
            {
                var classCategory = new ClassCategory
                {
                    Name = model.Name,
                    Description = model.Description,
                };

                if (model.ImageFile != null)
                {
                    classCategory.ImagePath = await _imageHelper.UploadImageAsync(model.ImageFile, "categories");
                }

                await _classCategoryRepository.CreateAsync(classCategory);
                return RedirectToAction("Index");
            }
            return View(model);
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
            var model = new ClassCategoriesViewModel
            {
                Id = classCategory.Id,
                Name = classCategory.Name,
                Description = classCategory.Description,
                ImagePath = classCategory.ImagePath
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClassCategoriesViewModel model)
        {
            ClassCategory oldCategory = await _classCategoryRepository.GetByIdAsync(model.Id);
            List<ClassCategory> categories = _classCategoryRepository.GetAll().ToList();

            foreach (var cat in categories)
            {
                if (cat.Name == model.Name && cat.Name != oldCategory.Name)
                {
                    ModelState.AddModelError("Name", "There is already a category with this name");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    oldCategory.Name = model.Name;
                    oldCategory.Description = model.Description;

                    if (model.ImageFile != null)
                    {
                        oldCategory.ImagePath = await _imageHelper.UploadImageAsync(model.ImageFile, "categories");
                    }
                    await _classCategoryRepository.UpdateAsync(oldCategory);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _classCategoryRepository.ExistsAsync(model.Id))
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
            return View(model);
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
