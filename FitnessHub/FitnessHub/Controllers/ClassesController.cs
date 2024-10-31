﻿using FitnessHub.Data.Entities;
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
                    Text = $"{instructor.FirstName} {instructor.LastName}",
                    Value = instructor.Id.ToString(),
                });
            }

            OnlineClassViewModel model = new OnlineClassViewModel
            {
                InstructorsList = selectInstructorList,
                CategoriesList = selectClassCategoriesList,
                DateStart = DateTime.UtcNow,
                DateEnd = DateTime.UtcNow.AddMinutes(60),
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

            if (string.IsNullOrEmpty(model.Platform))
            {
                ModelState.AddModelError("Platform", "Please enter the platform for the class");
            }

            if (model.DateEnd < model.DateStart)
            {
                ModelState.AddModelError("DateEnd", "The ending date must be after the starting date");
            }

            if (model.DateStart < DateTime.UtcNow)
            {
                ModelState.AddModelError("DateStart", "Please select a valid starting date");
            }

            if (model.DateEnd < DateTime.UtcNow)
            {
                ModelState.AddModelError("DateEnd", "Please select a valid ending date");
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

            List<Instructor> instructorsList = await _userHelper.GetUsersByTypeAsync<Instructor>();
            List<SelectListItem> selectInstructorList = new List<SelectListItem>();
            List<SelectListItem> selectClassCategoriesList = await _classCategoryRepository.GetCategoriesSelectListAsync();

            foreach (Instructor inst in instructorsList)
            {
                selectInstructorList.Add(new SelectListItem
                {
                    Text = $"{inst.FirstName} {inst.LastName}",
                    Value = inst.Id.ToString(),
                });
            }

            model.InstructorsList = selectInstructorList;
            model.CategoriesList = selectClassCategoriesList;

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

            List<Instructor> instructorsList = await _userHelper.GetUsersByTypeAsync<Instructor>();
            List<SelectListItem> selectInstructorList = new List<SelectListItem>();

            foreach (Instructor instructor in instructorsList)
            {
                selectInstructorList.Add(new SelectListItem
                {
                    Text = $"{instructor.FirstName} {instructor.LastName}",
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
                Category = onlineClass.Category,
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
            OnlineClass? onlineClass = await _classRepository.GetOnlineClassByIdInclude(model.Id);

            if (onlineClass == null)
            {
                return ClassNotFound();
            }

            List<Instructor> instructorsList = await _userHelper.GetUsersByTypeAsync<Instructor>();
            List<SelectListItem> selectInstructorList = new List<SelectListItem>();

            foreach (Instructor instructor in instructorsList)
            {
                selectInstructorList.Add(new SelectListItem
                {
                    Text = $"{instructor.FirstName} {instructor.LastName}",
                    Value = instructor.Id.ToString(),
                });
            }

            model.InstructorsList = selectInstructorList;
            model.Category = onlineClass.Category;

            if (string.IsNullOrEmpty(model.Platform))
            {
                ModelState.AddModelError("Platform", "Please enter the platform for the class");
            }

            if (model.DateEnd < model.DateStart)
            {
                ModelState.AddModelError("DateEnd", "The ending date must be after the starting date");
            }

            if (model.DateStart < DateTime.UtcNow)
            {
                ModelState.AddModelError("DateStart", "Please select a valid starting date");
            }

            if (model.DateEnd < DateTime.UtcNow)
            {
                ModelState.AddModelError("DateEnd", "Please select a valid ending date");
            }

            if (string.IsNullOrEmpty(model.InstructorId))
            {
                ModelState.AddModelError("InstructorId", "Please select a valid Instructor");
            }

            if (ModelState.IsValid)
            {
                onlineClass.DateStart = model.DateStart;
                onlineClass.DateEnd = model.DateEnd;
                onlineClass.Platform = model.Platform;
                onlineClass.Instructor = await _userHelper.GetUserByIdAsync(model.InstructorId) as Instructor;

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

            return View(model);
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
            }

            if (model.DateStart < DateTime.UtcNow)
            {
                ModelState.AddModelError("DateStart", "Please select a valid starting date");
            }

            if (model.DateEnd < DateTime.UtcNow)
            {
                ModelState.AddModelError("DateEnd", "Please select a valid ending date");
            }

            if (model.Capacity < 1)
            {
                ModelState.AddModelError("Capacity", "Please select a valid class Capacity");
            }

            if (string.IsNullOrEmpty(model.InstructorId))
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
                ModelState.AddModelError("CategoryId", "Category not found");
            }

            Instructor? instructor = await _userHelper.GetUserByIdAsync(model.InstructorId) as Instructor;

            if (instructor == null)
            {
                ModelState.AddModelError("InstructorId", "Instructor not found");
            }

            if (model.GymId < 1)
            {
                ModelState.AddModelError("GymId", "Please select a valid Gym");
            }

            Gym? gym = await _gymRepository.GetByIdTrackAsync(model.GymId);

            if (gym == null)
            {
                ModelState.AddModelError("GymId", "Gym not found");
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

            foreach (Instructor instructor in instructorsList)
            {
                selectInstructorList.Add(new SelectListItem
                {
                    Text = $"{instructor.FirstName} {instructor.LastName}",
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
                Category = gymClass.Category,
                Capacity = gymClass.Capacity,
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
            GymClass? gymClass = await _classRepository.GetGymClassByIdInclude(model.Id);

            if (gymClass == null)
            {
                return ClassNotFound();
            }

            List<Instructor> instructorsList = await _userHelper.GetUsersByTypeAsync<Instructor>();
            instructorsList = instructorsList.Where(i => i.GymId == gymClass.Gym.Id).ToList();
            List<SelectListItem> selectInstructorList = new List<SelectListItem>();

            foreach (Instructor inst in instructorsList)
            {
                selectInstructorList.Add(new SelectListItem
                {
                    Text = $"{inst.FirstName} {inst.LastName}",
                    Value = inst.Id.ToString(),
                });
            }

            model.InstructorsList = selectInstructorList;
            model.Category = gymClass.Category;
            model.Gym = await _gymRepository.GetByIdAsync(gymClass.Gym.Id);
            model.Capacity = gymClass.Capacity;

            if (model.DateEnd < model.DateStart)
            {
                ModelState.AddModelError("DateEnd", "The ending date must be after the starting date");
            }

            if (model.DateStart < DateTime.UtcNow)
            {
                ModelState.AddModelError("DateStart", "Please select a valid starting date");
            }

            if (model.DateEnd < DateTime.UtcNow)
            {
                ModelState.AddModelError("DateEnd", "Please select a valid ending date");
            }

            if (string.IsNullOrEmpty(model.InstructorId))
            {
                ModelState.AddModelError("InstructorId", "Please select a valid Instructor");
            }

            Instructor instructor = await _userHelper.GetUserByIdAsync(model.InstructorId) as Instructor;

            if (instructor == null)
            {
                ModelState.AddModelError("InstructorId", "Please select a valid Instructor");
            }

            ModelState.Remove("Gym.City");
            ModelState.Remove("Gym.Name");
            ModelState.Remove("Gym.Address");
            ModelState.Remove("Gym.Country");

            if (ModelState.IsValid)
            {
                gymClass.DateStart = model.DateStart;
                gymClass.DateEnd = model.DateEnd;
                gymClass.Instructor = instructor;

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
            List<VideoClass> videoClasses = await _classRepository.GetAllVideoClassesInclude();
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
        public async Task<IActionResult> CreateVideoClass()
        {
            List<SelectListItem> selectCategoryList = await _classCategoryRepository.GetCategoriesSelectListAsync();

            var model = new VideoClassViewModel
            {
                CategoriesList = selectCategoryList,
            };

            return View(model);
        }

        // POST: Classes/CreateVideoClass
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVideoClass(VideoClassViewModel model)
        {

            if (model.VideoClassUrl.Length > 4 && !model.VideoClassUrl.ToLower().Contains("youtu"))
            {
                ModelState.AddModelError("VideoClassUrl", "Only URL's from Youtube videos are accepted");
            }

            if (model.CategoryId < 1)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid Category");
            }

            ClassCategory category = await _classCategoryRepository.GetByIdTrackAsync(model.CategoryId);

            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Category not found");
            }

            if (ModelState.IsValid)
            {
                var videoClass = new VideoClass
                {
                    VideoClassUrl = model.VideoClassUrl,
                    Category = category,
                };

                await _classRepository.CreateAsync(videoClass);

                return RedirectToAction(nameof(VideoClasses));
            }

            List<SelectListItem> selectCategoryList = await _classCategoryRepository.GetCategoriesSelectListAsync();
            model.CategoriesList = selectCategoryList;

            return View(model);
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
            if (videoClass.VideoClassUrl.Length > 4 && !videoClass.VideoClassUrl.ToLower().Contains("youtu"))
            {
                ModelState.AddModelError("VideoClassUrl", "Only URL's from Youtube videos are accepted");
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
            List<Instructor> instructors = new();

            if (dateStart > dateEnd || dateStart < DateTime.UtcNow || dateEnd < DateTime.UtcNow)
            {
                return Json(instructors);
            }

            instructors = await _userHelper.GetUsersByTypeAsync<Instructor>();

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
            List<Instructor> instructors = new();

            if (dateStart > dateEnd || dateStart < DateTime.UtcNow || dateEnd < DateTime.UtcNow)
            {
                return Json(instructors);
            }

            instructors = await _userHelper.GetUsersByTypeAsync<Instructor>();

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
            List<Instructor> instructors = new();

            if (dateStart > dateEnd || dateStart < DateTime.UtcNow || dateEnd < DateTime.UtcNow)
            {
                return Json(instructors);
            }

            instructors = await _userHelper.GetUsersByTypeAsync<Instructor>();

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
            List<Instructor> instructors = new();

            if (dateStart > dateEnd || dateStart < DateTime.UtcNow || dateEnd < DateTime.UtcNow)
            {
                return Json(instructors);
            }

           instructors = await _userHelper.GetUsersByTypeAsync<Instructor>();

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
