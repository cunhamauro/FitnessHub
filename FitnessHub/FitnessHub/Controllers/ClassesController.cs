using FitnessHub.Data.Entities;
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
    [Authorize(Roles = "Admin")]
    public class ClassesController : Controller
    {
        private readonly IClassRepository _classRepository;
        private readonly IUserHelper _userHelper;
        private readonly IGymRepository _gymRepository;
        private readonly IClassCategoryRepository _classCategoryRepository;

        public ClassesController(IClassRepository classRepository, IUserHelper userHelper, IGymRepository gymRepository, IClassCategoryRepository classCategoryRepository)
        {
            _classRepository = classRepository;
            _userHelper = userHelper;
            _gymRepository = gymRepository;
            _classCategoryRepository = classCategoryRepository;
        }

        // Index not in use
        //// GET: Classes
        //public async Task<IActionResult> Index()
        //{
        //    return View(await _context.Class.ToListAsync());
        //}

        #region Online Classes

        // GET: OnlineClasses
        public async Task<IActionResult> OnlineClasses() // Index for Online Classes
        {
            List<OnlineClass> onlineClasses = await _classRepository.GetAllOnlineClassesInclude();
            return View(onlineClasses);
        }

        // GET: Classes/OnlineClassDetails/5
        public async Task<IActionResult> OnlineClassDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            OnlineClass? onlineClass = await _classRepository.GetOnlineClassByIdInclude(id.Value);

            if (onlineClass == null)
            {
                return NotFound();
            }

            return View(onlineClass);
        }

        // GET: Classes/CreateOnlineClass
        public async Task<IActionResult> CreateOnlineClass()
        {
            List<Instructor> instructorsList = await _userHelper.GetUsersByTypeAsync<Instructor>();
            List<SelectListItem> selectInstructorList = new List<SelectListItem>();
            List<SelectListItem> selectClassCategoriesList = await _classCategoryRepository.GetCategoriesSelectListAsync();

            foreach (Instructor instructor in instructorsList)
            {
                selectInstructorList.Add(new SelectListItem
                {
                    Text = $"{instructor.Id} - {instructor.FirstName} {instructor.LastName}",
                    Value = instructor.Id.ToString(),
                });
            }

            OnlineClassViewModel model = new OnlineClassViewModel
            {
                InstructorsList = selectInstructorList,
                CategoriesList = selectClassCategoriesList,
            };

            return View(model);
        }

        // POST: Classes/CreateOnlineClass
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOnlineClass(OnlineClassViewModel model)
        {
            if (string.IsNullOrEmpty(model.InstructorId))
            {
                ModelState.AddModelError("InstructorId", "Please select a valid Instructor");
            }

            if (model.CategoryId < 1)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid Category");
            }

            Instructor? instructor = await _userHelper.GetUserByIdAsync(model.InstructorId) as Instructor;

            ClassCategory? category = await _classCategoryRepository.GetByIdTrackAsync(model.CategoryId);

            if (instructor == null)
            {
                ModelState.AddModelError("InstructorId", "Instructor not found");
            }

            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Category not found");
            }

            if (ModelState.IsValid)
            {
                OnlineClass onlineClass = new OnlineClass
                {
                    Category = category,
                    Instructor = instructor,
                    DateStart = model.DateStart,
                    DateEnd = model.DateEnd,
                    Platform = model.Platform,
                };

                await _classRepository.CreateAsync(onlineClass);

                return RedirectToAction(nameof(OnlineClasses));
            }
            return View(model);
        }

        // GET: Classes/UpdateOnlineClass/5
        public async Task<IActionResult> UpdateOnlineClass(int? id)
        {
            if (id == null)
            {
                return ClassNotFound();
            }

            OnlineClass? onlineClass = await _classRepository.GetOnlineClassByIdInclude(id.Value);

            if (onlineClass == null)
            {
                return ClassNotFound();
            }

            List<SelectListItem> selectClassCategoriesList = await _classCategoryRepository.GetCategoriesSelectListAsync();
            List<Instructor> instructorsList = await _userHelper.GetUsersByTypeAsync<Instructor>();
            List<SelectListItem> selectInstructorList = new List<SelectListItem>();

            foreach (Instructor instructor in instructorsList)
            {
                selectInstructorList.Add(new SelectListItem
                {
                    Text = $"{instructor.Id} - {instructor.FirstName} {instructor.LastName}",
                    Value = instructor.Id.ToString(),
                });
            }

            OnlineClassViewModel model = new OnlineClassViewModel
            {
                Id = id.Value,
                InstructorsList = selectInstructorList,
                InstructorId = onlineClass.Instructor.Id,
                DateStart = onlineClass.DateStart,
                DateEnd = onlineClass.DateEnd,
                Platform = onlineClass.Platform,
                CategoryId = onlineClass.Category.Id,
                CategoriesList = selectClassCategoriesList,
            };

            return View(model);
        }

        // POST: Classes/UpdateOnlineClass/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOnlineClass(OnlineClassViewModel model)
        {
            if (string.IsNullOrEmpty(model.InstructorId))
            {
                ModelState.AddModelError("InstructorId", "Please select a valid Instructor");
            }

            if (model.CategoryId < 1)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid Category");
            }

            OnlineClass? onlineClass = await _classRepository.GetOnlineClassByIdInclude(model.Id);

            if (onlineClass == null)
            {
                return ClassNotFound();
            }

            onlineClass.Category = await _classCategoryRepository.GetByIdTrackAsync(model.Id);
            onlineClass.DateStart = model.DateStart;
            onlineClass.DateEnd = model.DateEnd;
            onlineClass.Platform = model.Platform;
            onlineClass.Instructor = await _userHelper.GetUserByIdAsync(model.InstructorId) as Instructor;

            if (ModelState.IsValid)
            {
                try
                {
                    await _classRepository.UpdateAsync(onlineClass);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _classRepository.ExistsAsync(onlineClass.Id))
                    {
                        return ClassNotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(OnlineClasses));
            }
            return View(onlineClass);
        }

        // GET: Classes/DeleteOnlineClass/5
        public async Task<IActionResult> DeleteOnlineClass(int? id)
        {
            if (id == null)
            {
                return ClassNotFound();
            }

            OnlineClass? onlineClass = await _classRepository.GetOnlineClassByIdInclude(id.Value);

            if (onlineClass == null)
            {
                return ClassNotFound();
            }

            return View(onlineClass);
        }

        // POST: Classes/DeleteOnlineClass/5
        [HttpPost, ActionName("DeleteOnlineClass")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOnlineClassConfirmed(int id)
        {
            OnlineClass? onlineClass = await _classRepository.GetOnlineClassByIdInclude(id);

            if (onlineClass != null)
            {
                await _classRepository.DeleteAsync(onlineClass);
            }

            return RedirectToAction(nameof(OnlineClasses));
        }

        #endregion

        #region Gym Classes

        // GET: GymClasses
        public async Task<IActionResult> GymClasses() // Index for Gym Classes
        {
            List<GymClass> gymClasses = await _classRepository.GetAllGymClassesInclude();
            return View(gymClasses);
        }

        // GET: Classes/GymClassDetails/5
        public async Task<IActionResult> GymClassDetails(int? id)
        {
            if (id == null)
            {
                return ClassNotFound();
            }

            GymClass? gymClass = await _classRepository.GetGymClassByIdInclude(id.Value);

            if (gymClass == null)
            {
                return ClassNotFound();
            }

            return View(gymClass);
        }

        // GET: Classes/CreateGymClass
        public async Task<IActionResult> CreateGymClass()
        {
            List<Gym> gymsList = await _gymRepository.GetAll().ToListAsync();
            List<SelectListItem> selectGymList = new List<SelectListItem>();
            List<SelectListItem> selectCategoryList = await _classCategoryRepository.GetCategoriesSelectListAsync();

            List<Instructor> instructorsList = await _userHelper.GetUsersByTypeAsync<Instructor>();
            List<SelectListItem> selectInstructorList = new List<SelectListItem>();

            foreach (Instructor instructor in instructorsList)
            {
                selectInstructorList.Add(new SelectListItem
                {
                    Text = $"{instructor.Id} - {instructor.FirstName} {instructor.LastName}",
                    Value = instructor.Id.ToString(),
                });
            }

            foreach (Gym gym in gymsList)
            {
                selectGymList.Add(new SelectListItem
                {
                    Text = $"{gym.Id} - {gym.Name} - {gym.City}, {gym.Country}",
                    Value = gym.Id.ToString(),
                });
            }

            GymClassViewModel model = new GymClassViewModel
            {
                InstructorsList = selectInstructorList,
                GymsList = selectGymList,
                CategoriesList = selectCategoryList,
                DateStart = DateTime.UtcNow,
                DateEnd = DateTime.UtcNow.AddMinutes(60),
            };

            return View(model);
        }

        // POST: Classes/CreateGymClass
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGymClass(GymClassViewModel model)
        {
            List<Gym> gymsList = await _gymRepository.GetAll().ToListAsync();
            List<SelectListItem> selectGymList = new List<SelectListItem>();
            List<SelectListItem> selectCategoryList = await _classCategoryRepository.GetCategoriesSelectListAsync();

            List<Instructor> instructorsList = await _userHelper.GetUsersByTypeAsync<Instructor>();
            List<SelectListItem> selectInstructorList = new List<SelectListItem>();

            foreach (Instructor inst in instructorsList)
            {
                selectInstructorList.Add(new SelectListItem
                {
                    Text = $"{inst.Id} - {inst.FirstName} {inst.LastName}",
                    Value = inst.Id.ToString(),
                });
            }

            foreach (Gym g in gymsList)
            {
                selectGymList.Add(new SelectListItem
                {
                    Text = $"{g.Id} - {g.Name} - {g.City}, {g.Country}",
                    Value = g.Id.ToString(),
                });
            }

            model.InstructorsList = selectInstructorList;
            model.GymsList = selectGymList;
            model.CategoriesList = selectCategoryList;

            if (model.DateEnd < model.DateStart)
            {
                ModelState.AddModelError("DateEnd", "The ending date must be after the starting date");
                return View(model);
            }

            if (model.DateStart < DateTime.UtcNow)
            {
                ModelState.AddModelError("DateStart", "Please select a valid starting date");
                return View(model);
            }

            if (model.DateEnd < DateTime.UtcNow)
            {
                ModelState.AddModelError("DateEnd", "Please select a valid ending date");
                return View(model);
            }

            if (model.Capacity < 1)
            {
                ModelState.AddModelError("Capacity", "Please select a valid class Capacity");
                return View(model);
            }

            if (string.IsNullOrEmpty(model.InstructorId))
            {
                ModelState.AddModelError("InstructorId", "Please select a valid Instructor");
                return View(model);
            }

            if (model.CategoryId < 1)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid Category");
                return View(model);
            }

            ClassCategory category = await _classCategoryRepository.GetByIdTrackAsync(model.CategoryId);

            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Category not found");
                return View(model);
            }

            Instructor? instructor = await _userHelper.GetUserByIdAsync(model.InstructorId) as Instructor;

            if (instructor == null)
            {
                ModelState.AddModelError("InstructorId", "Instructor not found");
                return View(model);
            }

            if (model.GymId < 1)
            {
                ModelState.AddModelError("GymId", "Please select a valid Gym");
                return View(model);
            }

            Gym? gym = await _gymRepository.GetByIdTrackAsync(model.GymId);

            if (gym == null)
            {
                ModelState.AddModelError("GymId", "Gym not found");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                GymClass gymClass = new GymClass
                {
                    Gym = gym,
                    Instructor = instructor,
                    DateStart = model.DateStart,
                    DateEnd = model.DateEnd,
                    Category = category,
                    Capacity = model.Capacity,
                };

                await _classRepository.CreateAsync(gymClass);
            }
            return RedirectToAction(nameof(GymClasses));
        }

        // GET: Classes/UpdateGymClass/5
        public async Task<IActionResult> UpdateGymClass(int? id)
        {
            if (id == null)
            {
                return ClassNotFound();
            }

            GymClass? gymClass = await _classRepository.GetGymClassByIdInclude(id.Value);

            if (gymClass == null)
            {
                return ClassNotFound();
            }

            List<Instructor> instructorsList = await _userHelper.GetUsersByTypeAsync<Instructor>();
            instructorsList = instructorsList.Where(i => i.GymId == gymClass.Gym.Id).ToList();
            List<SelectListItem> selectInstructorList = new List<SelectListItem>();
            List<SelectListItem> selectCategoriesList = await _classCategoryRepository.GetCategoriesSelectListAsync();

            foreach (Instructor instructor in instructorsList)
            {
                selectInstructorList.Add(new SelectListItem
                {
                    Text = $"{instructor.Id} - {instructor.FirstName} {instructor.LastName}",
                    Value = instructor.Id.ToString(),
                });
            }

            GymClassViewModel model = new GymClassViewModel
            {
                Id = gymClass.Id,
                InstructorId = gymClass.Instructor.Id,
                Gym = await _gymRepository.GetByIdAsync(gymClass.Gym.Id),
                InstructorsList = selectInstructorList,
                DateStart = gymClass.DateStart,
                DateEnd = gymClass.DateEnd,
                CategoryId = gymClass.Category.Id,
                CategoriesList = selectCategoriesList,
            };

            return View(model);
        }

        // POST: Classes/UpdateGymClass/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGymClass(GymClassViewModel model)
        {
            if (string.IsNullOrEmpty(model.InstructorId))
            {
                ModelState.AddModelError("InstructorId", "Please select a valid Instructor");
            }

            Instructor instructor = await _userHelper.GetUserByIdAsync(model.InstructorId) as Instructor;

            if (instructor == null)
            {
                ModelState.AddModelError("InstructorId", "Please select a valid Instructor");
            }

            if (model.CategoryId < 1)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid Category");
            }

            ClassCategory category = await _classCategoryRepository.GetByIdTrackAsync(model.CategoryId);

            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid Category");
            }

            GymClass? gymClass = await _classRepository.GetGymClassByIdInclude(model.Id);

            if (gymClass == null)
            {
                return ClassNotFound();
            }

            gymClass.DateStart = model.DateStart;
            gymClass.DateEnd = model.DateEnd;
            gymClass.Instructor = instructor;
            gymClass.Category = category;

            ModelState.Remove("Gym.City");
            ModelState.Remove("Gym.Name");
            ModelState.Remove("Gym.Address");
            ModelState.Remove("Gym.Country");

            if (ModelState.IsValid)
            {
                try
                {
                    await _classRepository.UpdateAsync(gymClass);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _classRepository.ExistsAsync(model.Id))
                    {
                        return ClassNotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(GymClasses));
            }
            return View(model);
        }

        // GET: Classes/DeleteGymClass/5
        public async Task<IActionResult> DeleteGymClass(int? id)
        {
            if (id == null)
            {
                return ClassNotFound();
            }

            GymClass? gymClass = await _classRepository.GetGymClassByIdInclude(id.Value);

            if (gymClass == null)
            {
                return ClassNotFound();
            }

            return View(gymClass);
        }

        // POST: Classes/DeleteGymClass/5
        [HttpPost, ActionName("DeleteGymClass")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGymClassConfirmed(int id)
        {
            GymClass? gymClass = await _classRepository.GetGymClassByIdInclude(id);

            if (gymClass != null)
            {
                await _classRepository.DeleteAsync(gymClass);
            }

            return RedirectToAction(nameof(GymClasses));
        }

        #endregion

        #region Video Classes

        // GET: VideoClasses
        public async Task<IActionResult> VideoClasses() // Index for Video Classes
        {
            List<VideoClass> videoClasses = await _classRepository.GetAllVideoClasses();
            return View(videoClasses);
        }

        // GET: Classes/VideoClassDetails/5
        public async Task<IActionResult> VideoClassDetails(int? id)
        {
            if (id == null)
            {
                return ClassNotFound();
            }

            // Fetch the class using the repository method
            VideoClass? videoClass = await _classRepository.GetVideoClassByIdInclude(id.Value);

            if (videoClass == null)
            {
                return ClassNotFound();
            }

            return View(videoClass);
        }

        // GET: Classes/CreateVideoClass
        public IActionResult CreateVideoClass()
        {
            return View();
        }

        // POST: Classes/CreateVideoClass
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVideoClass(VideoClass videoClass)
        {
            if (ModelState.IsValid)
            {
                await _classRepository.CreateAsync(videoClass);

                return RedirectToAction(nameof(VideoClasses));
            }
            return View(videoClass);
        }

        // GET: Classes/UpdateVideoClass/5
        public async Task<IActionResult> UpdateVideoClass(int? id)
        {
            if (id == null)
            {
                return ClassNotFound();
            }

            VideoClass? videoClass = await _classRepository.GetVideoClassByIdInclude(id.Value);

            if (videoClass == null)
            {
                return ClassNotFound();
            }
            return View(videoClass);
        }

        // POST: Classes/UpdateVideoClass/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVideoClass(VideoClass videoClass)
        {
            if (videoClass == null)
            {
                return ClassNotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _classRepository.UpdateAsync(videoClass);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _classRepository.ExistsAsync(videoClass.Id))
                    {
                        return ClassNotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(VideoClasses));
            }
            return View(videoClass);
        }

        // GET: Classes/DeleteVideoClass/5
        public async Task<IActionResult> DeleteVideoClass(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            VideoClass? videoClass = await _classRepository.GetVideoClassByIdInclude(id.Value);

            if (videoClass == null)
            {
                return ClassNotFound();
            }

            return View(videoClass);
        }

        // POST: Classes/DeleteVideoClass/5
        [HttpPost, ActionName("DeleteVideoClass")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVideoClassConfirmed(int id)
        {
            VideoClass? videoClass = await _classRepository.GetVideoClassByIdInclude(id);

            if (videoClass != null)
            {
                await _classRepository.DeleteAsync(videoClass);
            }

            return RedirectToAction(nameof(VideoClasses));
        }

        #endregion

        #region AJAX Instructors Class Availability 

        public async Task<JsonResult> GetAvailableInstructorsOnline(DateTime dateStart, DateTime dateEnd)
        {
            // Get all instructors
            List<Instructor> instructors = await _userHelper.GetUsersByTypeAsync<Instructor>();

            // Get all online classes
            List<OnlineClass> onlineClasses = await _classRepository.GetAll().OfType<OnlineClass>().ToListAsync();

            // Iterate through each online class to check for overlaps
            foreach (OnlineClass onlineClass in onlineClasses)
            {
                // Check if the class overlaps with the requested time range
                if (!(onlineClass.DateEnd < dateStart || onlineClass.DateStart > dateEnd))
                {
                    // The class overlaps remove its instructor from the available list
                    instructors.Remove(onlineClass.Instructor);
                }
            }

            List<GymClass> gymClasses = await _classRepository.GetAll().OfType<GymClass>().ToListAsync();

            // Iterate through each gym class to check for overlaps
            foreach (GymClass gymClass in gymClasses)
            {
                // Check if the class overlaps with the requested time range
                if (!(gymClass.DateEnd < dateStart || gymClass.DateStart > dateEnd))
                {
                    // If the class overlaps remove its instructor from the available list
                    instructors.Remove(gymClass.Instructor);
                }
            }

            // Return the filtered list of available instructors
            return Json(instructors);
        }

        public async Task<JsonResult> GetAvailableInstructorsGym(DateTime dateStart, DateTime dateEnd, int gymSelect)
        {
            // Get all instructors
            List<Instructor> instructors = await _userHelper.GetUsersByTypeAsync<Instructor>();

            // Get all online classes
            List<OnlineClass> onlineClasses = await _classRepository.GetAll().OfType<OnlineClass>().ToListAsync();

            // Iterate through each online class to check for overlaps
            foreach (OnlineClass onlineClass in onlineClasses)
            {
                // Check if the class overlaps with the requested time range
                if (!(onlineClass.DateEnd < dateStart || onlineClass.DateStart > dateEnd))
                {
                    // The class overlaps remove its instructor from the available list
                    instructors.Remove(onlineClass.Instructor);
                }
            }

            List<GymClass> gymClasses = await _classRepository.GetAll().OfType<GymClass>().ToListAsync();

            // Iterate through each gym class to check for overlaps
            foreach (GymClass gymClass in gymClasses)
            {
                // Check if the class overlaps with the requested time range
                if (!(gymClass.DateEnd < dateStart || gymClass.DateStart > dateEnd))
                {
                    // If the class overlaps remove its instructor from the available list
                    instructors.Remove(gymClass.Instructor);
                }
            }

            instructors = instructors.Where(i => i.GymId == gymSelect).ToList();

            // Return the filtered list of available instructors
            return Json(instructors);
        }

        public async Task<JsonResult> GetAvailableInstructorsOnlineEdit(DateTime dateStart, DateTime dateEnd, int classId)
        {
            // Get all instructors
            List<Instructor> instructors = await _userHelper.GetUsersByTypeAsync<Instructor>();

            // Get all online classes
            List<OnlineClass> onlineClasses = await _classRepository.GetAll().OfType<OnlineClass>().ToListAsync();

            // Iterate through each online class to check for overlaps
            foreach (OnlineClass onlineClass in onlineClasses)
            {
                // Check if the class overlaps with the requested time range
                if (!(onlineClass.DateEnd < dateStart || onlineClass.DateStart > dateEnd) && onlineClass.Id != classId)
                {
                    // The class overlaps remove its instructor from the available list
                    instructors.Remove(onlineClass.Instructor);
                }
            }

            List<GymClass> gymClasses = await _classRepository.GetAll().OfType<GymClass>().ToListAsync();

            // Iterate through each gym class to check for overlaps
            foreach (GymClass gymClass in gymClasses)
            {
                // Check if the class overlaps with the requested time range
                if (!(gymClass.DateEnd < dateStart || gymClass.DateStart > dateEnd))
                {
                    // If the class overlaps remove its instructor from the available list
                    instructors.Remove(gymClass.Instructor);
                }
            }

            // Return the filtered list of available instructors
            return Json(instructors);
        }

        public async Task<JsonResult> GetAvailableInstructorsGymEdit(DateTime dateStart, DateTime dateEnd, int gymId, int classId)
        {
            // Get all instructors
            List<Instructor> instructors = await _userHelper.GetUsersByTypeAsync<Instructor>();

            // Get all online classes
            List<OnlineClass> onlineClasses = await _classRepository.GetAll().OfType<OnlineClass>().ToListAsync();

            // Iterate through each online class to check for overlaps
            foreach (OnlineClass onlineClass in onlineClasses)
            {
                // Check if the class overlaps with the requested time range
                if (!(onlineClass.DateEnd < dateStart || onlineClass.DateStart > dateEnd))
                {
                    // The class overlaps remove its instructor from the available list
                    instructors.Remove(onlineClass.Instructor);
                }
            }

            List<GymClass> gymClasses = await _classRepository.GetAll().OfType<GymClass>().ToListAsync();

            // Iterate through each gym class to check for overlaps
            foreach (GymClass gymClass in gymClasses)
            {
                // Check if the class overlaps with the requested time range
                if (!(gymClass.DateEnd < dateStart || gymClass.DateStart > dateEnd) && gymClass.Id != classId)
                {
                    // If the class overlaps remove its instructor from the available list
                    instructors.Remove(gymClass.Instructor);
                }
            }

            instructors = instructors.Where(i => i.GymId == gymId).ToList();

            // Return the filtered list of available instructors
            return Json(instructors);
        }

        #endregion

        public IActionResult ClassNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Class not found", Message = "With so many available, how could you not find one?" });
        }

        public IActionResult CategoryNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Category not found", Message = "Maybe it got lost at the gym?" });
        }

        public IActionResult GymNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Gym not found", Message = "With so many worldwide, how did you miss this one?" });
        }

        [Authorize(Roles = "Client, Admin, MasterAdmin, Instructor, Employee")]
        [AllowAnonymous]
        public async Task<IActionResult> Available()
        {
            var categories = await _classCategoryRepository.GetAll().ToListAsync();
            return View(categories);
        }
    }
}
