using FitnessHub.Data.Entities.GymClasses;
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
    public class ClassTypesController : Controller
    {
        private readonly IClassTypeRepository _classTypeRepository;
        private readonly IImageHelper _imageHelper;
        private readonly IClassCategoryRepository _classCategoryRepository;
        private readonly IClassRepository _classRepository;

        public ClassTypesController(IClassTypeRepository classTypeRepository, IImageHelper imageHelper, IClassCategoryRepository classCategoryRepository, IClassRepository classRepository)
        {
            _classTypeRepository = classTypeRepository;
            _imageHelper = imageHelper;
            _classCategoryRepository = classCategoryRepository;
            _classRepository = classRepository;
        }

        // GET: ClassTypes
        public IActionResult Index()
        {
            return View(_classTypeRepository.GetAll().Include(t => t.ClassCategory).OrderBy(c => c.Name));
        }

        // GET: ClassTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return TypeNotFound();
            }

            var classType = await _classTypeRepository.GetByIdIncludeCategory(id.Value);

            if (classType == null)
            {
                return TypeNotFound();
            }

            return View(classType);
        }

        // GET: ClassTypes/Create
        public async Task<IActionResult> Create()
        {
            List<SelectListItem> selectCategory = new List<SelectListItem>();

            List<ClassCategory> categoriesList = await _classCategoryRepository.GetAll().ToListAsync();

            foreach (var type in categoriesList)
            {
                selectCategory.Add(new SelectListItem
                {
                    Text = type.Name,
                    Value = type.Id.ToString(),
                });
            }

            ClassTypeViewModel model = new()
            {
                SelectListCategories = selectCategory,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassTypeViewModel model)
        {
            List<SelectListItem> selectCategory = new List<SelectListItem>();

            List<ClassCategory> categoriesList = await _classCategoryRepository.GetAll().ToListAsync();

            foreach (var type in categoriesList)
            {
                selectCategory.Add(new SelectListItem
                {
                    Text = type.Name,
                    Value = type.Id.ToString(),
                });
            }

            model.SelectListCategories = selectCategory;

            List<ClassType> types = _classTypeRepository.GetAll().ToList();

            foreach (var tp in types)
            {
                if (tp.Name == model.Name)
                {
                    ModelState.AddModelError("Name", "There is already a class type with this name");
                }
            }

            if (model.CategoryId < 1)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid class category");
            }

            var classCategory = await _classCategoryRepository.GetByIdTrackAsync(model.CategoryId);

            if (classCategory == null)
            {
                ModelState.AddModelError("TypeId", "Please select a valid class category");
            }

            ClassType classType = new()
            {
                Name = model.Name,
                Description = model.Description,
                ClassCategory = classCategory,
            };

            if (model.ImageFile != null)
            {
                classType.ImagePath = await _imageHelper.UploadImageAsync(model.ImageFile, "types");
            }

            if (ModelState.IsValid)
            {
                await _classTypeRepository.CreateAsync(classType);
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return TypeNotFound();
            }

            var classType = await _classTypeRepository.GetByIdAsync(id.Value);

            if (classType == null)
            {
                return TypeNotFound();
            }
            var model = new ClassTypeViewModel
            {
                Id = classType.Id,
                Name = classType.Name,
                Description = classType.Description,
                ImagePath = classType.ImagePath
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClassTypeViewModel model)
        {
            ClassType oldType = await _classTypeRepository.GetByIdAsync(model.Id);
            List<ClassType> type = _classTypeRepository.GetAll().ToList();

            foreach (var cat in type)
            {
                if (cat.Name == model.Name && cat.Name != oldType.Name)
                {
                    ModelState.AddModelError("Name", "There is already a class type with this name");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    oldType.Name = model.Name;

                    oldType.Description = model.Description;

                    if (model.ImageFile != null)
                    {
                        oldType.ImagePath = await _imageHelper.UploadImageAsync(model.ImageFile, "types");
                    }
                    await _classTypeRepository.UpdateAsync(oldType);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _classTypeRepository.ExistsAsync(model.Id))
                    {
                        return TypeNotFound();
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

        // GET: ClassType/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return TypeNotFound();
            }

            var classType = await _classTypeRepository.GetByIdAsync(id.Value);

            if (classType == null)
            {
                return TypeNotFound();
            }

            return View(classType);
        }

        // POST: ClassType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var classType = await _classTypeRepository.GetByIdAsync(id);

            if (classType == null)
            {
                return TypeNotFound();
            }

            var classesWithThisType = await _classRepository.GetAll()
            .Where(c => c.ClassType.Id == classType.Id)
            .ToListAsync();

            if (classesWithThisType.Any())
            {
                ModelState.AddModelError(string.Empty, "You cannot delete this class type because it is associated with one or more classes.");
                return View("Details", classType);
                
            }
            await _classTypeRepository.DeleteAsync(classType);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult TypeNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Class type not found", Message = "Not enough options for you?" });
        }

        public IActionResult DisplayMessage(string title, string message)
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = $"{title}", Message = $"{message}" });
        }
    }
}
