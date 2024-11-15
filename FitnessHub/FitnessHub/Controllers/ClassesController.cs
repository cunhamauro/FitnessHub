using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.GymClasses;
using FitnessHub.Data.Entities.History;
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
    public class ClassesController : Controller
    {
        private readonly IClassRepository _classRepository;
        private readonly IUserHelper _userHelper;
        private readonly IGymRepository _gymRepository;
        private readonly IClassCategoryRepository _classCategoryRepository;
        private readonly IClassHistoryRepository _classHistoryRepository;
        private readonly IRegisteredInClassesHistoryRepository _registeredInClassesHistoryRepository;
        private readonly IClassTypeRepository _classTypeRepository;
        private readonly IClientHistoryRepository _clientHistoryRepository;
        private readonly IStaffHistoryRepository _staffHistoryRepository;

        public ClassesController(IClassRepository classRepository, IUserHelper userHelper, IGymRepository gymRepository, IClassCategoryRepository classCategoryRepository, IClassHistoryRepository classHistoryRepository, IRegisteredInClassesHistoryRepository registeredInClassesHistoryRepository, IClassTypeRepository classTypeRepository, IClientHistoryRepository clientHistoryRepository, IStaffHistoryRepository staffHistoryRepository)
        {
            _classRepository = classRepository;
            _userHelper = userHelper;
            _gymRepository = gymRepository;
            _classCategoryRepository = classCategoryRepository;
            _classHistoryRepository = classHistoryRepository;
            _registeredInClassesHistoryRepository = registeredInClassesHistoryRepository;
            _classTypeRepository = classTypeRepository;
            _clientHistoryRepository = clientHistoryRepository;
            _staffHistoryRepository = staffHistoryRepository;
        }

        // Index not in use
        //// GET: Classes
        //public async Task<IActionResult> Index()
        //{
        //    return View(await _context.Class.ToListAsync());
        //}

        [Authorize(Roles = "Admin, Employee")]
        public async Task<IActionResult> ClassesHistory()
        {
            List<ClassHistory> classes = await _classHistoryRepository.GetAll().ToListAsync();
            List<ClassHistoryViewModel> classesHistory = new();

            List<RegisteredInClassesHistory> registrations = await _registeredInClassesHistoryRepository.GetAll().ToListAsync();

            foreach (var ch in classes)
            {
                List<string> clientEmailsList = new List<string>();

                foreach (var registration in registrations)
                {
                    if (registration.ClassId == ch.Id)
                    {
                        var registeredUser = await _clientHistoryRepository.GetByIdTrackAsync(registration.UserId);
                        clientEmailsList.Add(registeredUser.Email);
                    }
                }

                StaffHistory instructor = await _staffHistoryRepository.GetByIdTrackAsync(ch.InstructorId);

                classesHistory.Add(new ClassHistoryViewModel
                {
                    ClientList = clientEmailsList,
                    InstructorFullName = instructor != null ? $"{instructor.FirstName} {instructor.LastName}" : string.Empty,
                    InstructorEmail = instructor?.Email ?? string.Empty,
                    Category = ch.Category,
                    Id = ch.Id,
                    SubClass = ch.SubClass,
                    Capacity = ch.Capacity,
                    VideoClassUrl = ch.VideoClassUrl,
                    DateStart = ch.DateStart,
                    DateEnd = ch.DateEnd,
                    GymName = ch.GymName,
                    Platform = ch.Platform,
                    InstructorId = ch.InstructorId,
                    Canceled = ch.Canceled,
                });
            }

            return View(classesHistory);
        }

        #region Online Classes

        // GET: OnlineClasses
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> OnlineClasses() // Index for Online Classes
        {
            List<OnlineClass> onlineClasses = await _classRepository.GetAllOnlineClassesInclude();
            return View(onlineClasses);
        }

        // GET: Classes/OnlineClassDetails/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> OnlineClassDetails(int? id)
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

        // GET: Classes/CreateOnlineClass
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateOnlineClass()
        {
            List<Instructor> instructorsList = await _userHelper.GetUsersByTypeAsync<Instructor>();
            List<SelectListItem> selectInstructorList = new List<SelectListItem>();
            List<SelectListItem> selectClassCategoriesList = await _classCategoryRepository.GetCategoriesSelectListAsync();
            List<SelectListItem> selectClassTypeList = await _classTypeRepository.GetTypesSelectListAsync();

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
                ClassTypeList = selectClassTypeList,
                DateStart = DateTime.UtcNow,
                DateEnd = DateTime.UtcNow.AddMinutes(60),
            };

            Admin admin = await _userHelper.GetUserAsync(this.User) as Admin;

            ViewBag.GymId = admin.GymId;

            return View(model);
        }

        // POST: Classes/CreateOnlineClass
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOnlineClass(OnlineClassViewModel model)
        {
            if (string.IsNullOrEmpty(model.InstructorId))
            {
                ModelState.AddModelError("InstructorId", "Please select a valid instructor");
            }

            if (model.CategoryId < 1)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid category");
            }

            if (model.ClassTypeId < 1)
            {
                ModelState.AddModelError("ClassTypeId", "Please select a valid class type");
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

            ClassType? type = await _classTypeRepository.GetByIdTrackAsync(model.ClassTypeId);

            if (instructor == null)
            {
                ModelState.AddModelError("InstructorId", "Instructor not found");
            }

            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Category not found");
            }

            if (type == null)
            {
                ModelState.AddModelError("ClassTypeId", "Class type not found");
            }

            if (ModelState.IsValid)
            {
                OnlineClass onlineClass = new OnlineClass
                {
                    Category = category,
                    ClassType = type,
                    Instructor = instructor,
                    DateStart = model.DateStart,
                    DateEnd = model.DateEnd,
                    Platform = model.Platform,
                };

                await _classRepository.CreateAsync(onlineClass);

                ClassHistory record = new ClassHistory()
                {
                    Id = onlineClass.Id,
                    ClassType = onlineClass.ClassType.Name,
                    Category = onlineClass.Category.Name,
                    SubClass = "OnlineClass",
                    DateStart = model.DateStart,
                    DateEnd = model.DateEnd,
                    Platform = model.Platform,
                    InstructorId = instructor.Id,
                };

                await _classHistoryRepository.CreateAsync(record);

                return RedirectToAction(nameof(OnlineClasses));
            }

            List<Instructor> instructorsList = await _userHelper.GetUsersByTypeAsync<Instructor>();
            List<SelectListItem> selectInstructorList = new List<SelectListItem>();
            List<SelectListItem> selectClassCategoriesList = await _classCategoryRepository.GetCategoriesSelectListAsync();
            List<SelectListItem> selectClassTypeList = await _classTypeRepository.GetTypesSelectListAsync();

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
            model.ClassTypeList = selectClassTypeList;

            return View(model);
        }

        // GET: Classes/UpdateOnlineClass/5
        [Authorize(Roles = "Admin")]
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
                ClassType = onlineClass.ClassType,
            };

            Admin admin = await _userHelper.GetUserAsync(this.User) as Admin;

            ViewBag.GymId = admin.GymId;

            return View(model);
        }

        // POST: Classes/UpdateOnlineClass/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
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
            model.ClassType = onlineClass.ClassType;

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

                var instructor = await _userHelper.GetUserByIdAsync(model.InstructorId) as Instructor;

                onlineClass.Instructor = instructor;

                ClassHistory record = await _classHistoryRepository.GetByIdAsync(onlineClass.Id);

                record.InstructorId = instructor.Id;
                record.DateStart = model.DateStart;
                record.DateEnd = model.DateEnd;
                record.Platform = model.Platform;

                try
                {
                    await _classRepository.UpdateAsync(onlineClass);

                    await _classHistoryRepository.UpdateAsync(record);
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteOnlineClass")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOnlineClassConfirmed(int id)
        {
            OnlineClass? onlineClass = await _classRepository.GetOnlineClassByIdInclude(id);

            if (onlineClass != null)
            {
                if (onlineClass.Clients == null)
                {
                    return ClassNotFound();
                }

                if (onlineClass.Clients.Count > 0)
                {
                    ModelState.AddModelError(string.Empty, "This class cannot be deleted because it has registered clients.");

                    return View("GymClassDetails", onlineClass);
                }
                await _classRepository.DeleteAsync(onlineClass);

                ClassHistory record = await _classHistoryRepository.GetByIdAsync(onlineClass.Id);

                record.Canceled = true;

                await _classHistoryRepository.UpdateAsync(record);
            }

            return RedirectToAction(nameof(OnlineClasses));
        }

        #endregion

        #region Gym Classes

        // GET: GymClasses
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GymClasses() // Index for Gym Classes
        {
            List<GymClass> gymClasses = await _classRepository.GetAllGymClassesInclude();
            return View(gymClasses);
        }

        // GET: Classes/GymClassDetails/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateGymClass()
        {
            Admin thisAdmin = await _userHelper.GetUserAsync(this.User) as Admin;

            List<SelectListItem> selectCategoryList = await _classCategoryRepository.GetCategoriesSelectListAsync();
            List<SelectListItem> selectClassTypeList = await _classTypeRepository.GetTypesSelectListAsync();

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

            GymClassViewModel model = new GymClassViewModel
            {
                InstructorsList = selectInstructorList,
                CategoriesList = selectCategoryList,
                ClassTypeList = selectClassTypeList,
                DateStart = DateTime.UtcNow,
                DateEnd = DateTime.UtcNow.AddMinutes(60),
                GymId = thisAdmin.GymId.Value,
            };

            return View(model);
        }

        // POST: Classes/CreateGymClass
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGymClass(GymClassViewModel model)
        {
            List<SelectListItem> selectCategoryList = await _classCategoryRepository.GetCategoriesSelectListAsync();
            List<SelectListItem> selectClassTypeList = await _classTypeRepository.GetTypesSelectListAsync();

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

            model.InstructorsList = selectInstructorList;
            model.CategoriesList = selectCategoryList;
            model.ClassTypeList = selectClassTypeList;

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
                ModelState.AddModelError("Capacity", "Please select a valid class capacity");
            }

            if (string.IsNullOrEmpty(model.InstructorId))
            {
                ModelState.AddModelError("InstructorId", "Please select a valid instructor");
            }

            if (model.CategoryId < 1)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid category");
            }

            ClassCategory? category = await _classCategoryRepository.GetByIdTrackAsync(model.CategoryId);

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
                ModelState.AddModelError("GymId", "Please select a valid gym");
            }

            Gym? gym = await _gymRepository.GetByIdTrackAsync(model.GymId);

            if (gym == null)
            {
                ModelState.AddModelError("GymId", "Gym not found");
            }

            if (model.ClassTypeId < 1)
            {
                ModelState.AddModelError("ClassTypeId", "Please select a valid class type");
            }

            ClassType? type = await _classTypeRepository.GetByIdTrackAsync(model.ClassTypeId);

            if (type == null)
            {
                ModelState.AddModelError("ClassTypeId", "Class type not found");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            GymClass gymClass = new GymClass
            {
                Gym = gym,
                Instructor = instructor,
                DateStart = model.DateStart,
                DateEnd = model.DateEnd,
                Category = category,
                ClassType = type,
                Capacity = model.Capacity,
            };

            await _classRepository.CreateAsync(gymClass);

            ClassHistory record = new ClassHistory()
            {
                Id = gymClass.Id,
                GymName = gym.Name,
                ClassType = type.Name,
                SubClass = "GymClass",
                Category = category.Name,
                InstructorId = instructor.Id,
                DateStart = model.DateStart,
                DateEnd = model.DateEnd,
                Capacity = model.Capacity,
            };

            await _classHistoryRepository.CreateAsync(record);

            return RedirectToAction(nameof(GymClasses));
        }

        // GET: Classes/UpdateGymClass/5
        [Authorize(Roles = "Admin")]
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
                ClassType = gymClass.ClassType,
                GymId = gymClass.Id,
            };

            Admin admin = await _userHelper.GetUserAsync(this.User) as Admin;

            return View(model);
        }

        // POST: Classes/UpdateGymClass/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
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
            model.ClassType = gymClass.ClassType;

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

                ClassHistory record = await _classHistoryRepository.GetByIdAsync(gymClass.Id);

                record.InstructorId = instructor.Id;
                record.DateStart = model.DateStart;
                record.DateEnd = model.DateEnd;

                try
                {
                    await _classRepository.UpdateAsync(gymClass);

                    await _classHistoryRepository.UpdateAsync(record);
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteGymClass")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGymClassConfirmed(int id)
        {
            GymClass? gymClass = await _classRepository.GetGymClassByIdInclude(id);

            if (gymClass != null)
            {
                if (gymClass.Clients == null)
                {
                    return ClassNotFound();
                }

                if(gymClass.Clients.Count > 0)
                {
                    ModelState.AddModelError(string.Empty, "This class cannot be deleted because it has registered clients.");

                    return View("GymClassDetails", gymClass);
                }

                await _classRepository.DeleteAsync(gymClass);

                ClassHistory record = await _classHistoryRepository.GetByIdAsync(gymClass.Id);

                record.Canceled = true;

                await _classHistoryRepository.UpdateAsync(record);
            }

            return RedirectToAction(nameof(GymClasses));
        }

        #endregion

        #region Video Classes

        // GET: VideoClasses
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VideoClasses() // Index for Video Classes
        {
            List<VideoClass> videoClasses = await _classRepository.GetAllVideoClassesInclude();
            return View(videoClasses);
        }

        // GET: Classes/VideoClassDetails/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateVideoClass()
        {
            List<SelectListItem> selectCategoryList = await _classCategoryRepository.GetCategoriesSelectListAsync();
            List<SelectListItem> selectClassTypeList = await _classTypeRepository.GetTypesSelectListAsync();

            var model = new VideoClassViewModel
            {
                CategoriesList = selectCategoryList,
                ClassTypeList = selectClassTypeList,
            };

            return View(model);
        }

        // POST: Classes/CreateVideoClass
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVideoClass(VideoClassViewModel model)
        {
            if (string.IsNullOrEmpty(model.VideoClassUrl))
            {
                ModelState.AddModelError("VideoClassUrl", "Please enter a Youtube video URL");
            }

            if (model.VideoClassUrl.Length < 4 || !model.VideoClassUrl.ToLower().Contains("youtu"))
            {
                ModelState.AddModelError("VideoClassUrl", "Only URL's from Youtube videos are accepted");
            }

            if (model.CategoryId < 1)
            {
                ModelState.AddModelError("CategoryId", "Please select a valid category");
            }

            ClassCategory category = await _classCategoryRepository.GetByIdTrackAsync(model.CategoryId);

            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Category not found");
            }

            if (model.ClassTypeId < 1)
            {
                ModelState.AddModelError("ClassTypeId", "Please select a valid class type");
            }

            ClassType? type = await _classTypeRepository.GetByIdTrackAsync(model.ClassTypeId);

            if (type == null)
            {
                ModelState.AddModelError("ClassTypeId", "Class type not found");
            }

            if (ModelState.IsValid)
            {
                var videoClass = new VideoClass
                {
                    VideoClassUrl = model.VideoClassUrl,
                    Category = category,
                    ClassType = type,
                };

                await _classRepository.CreateAsync(videoClass);

                ClassHistory record = new ClassHistory()
                {
                    Id = videoClass.Id,
                    SubClass = "VideoClass",
                    Category = category.Name,
                    VideoClassUrl = model.VideoClassUrl,
                    ClassType = type.Name,
                };

                await _classHistoryRepository.CreateAsync(record);

                return RedirectToAction(nameof(VideoClasses));
            }

            List<SelectListItem> selectCategoryList = await _classCategoryRepository.GetCategoriesSelectListAsync();
            List<SelectListItem> selectClassTypeList = await _classTypeRepository.GetTypesSelectListAsync();

            model.ClassTypeList = selectClassTypeList;
            model.CategoriesList = selectCategoryList;

            return View(model);
        }

        // GET: Classes/UpdateVideoClass/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
                ClassHistory record = await _classHistoryRepository.GetByIdAsync(videoClass.Id);

                record.VideoClassUrl = videoClass.VideoClassUrl;

                try
                {
                    await _classRepository.UpdateAsync(videoClass);

                    await _classHistoryRepository.UpdateAsync(record);
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

            videoClass = await _classRepository.GetVideoClassByIdInclude(videoClass.Id); // ??? Regen ???

            return View(videoClass);
        }

        // GET: Classes/DeleteVideoClass/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVideoClass(int? id)
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

        // POST: Classes/DeleteVideoClass/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteVideoClass")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVideoClassConfirmed(int id)
        {
            VideoClass? videoClass = await _classRepository.GetVideoClassByIdInclude(id);

            if (videoClass != null)
            {
                await _classRepository.DeleteAsync(videoClass);

                ClassHistory record = await _classHistoryRepository.GetByIdAsync(videoClass.Id);

                record.Canceled = true;

                await _classHistoryRepository.UpdateAsync(record);
            }

            return RedirectToAction(nameof(VideoClasses));
        }

        #endregion

        #region AJAX Instructors Class Availability 

        public async Task<JsonResult> GetClassTypesFromCategory(int categoryId)
        {
            List<ClassType> classTypes = new List<ClassType>();

            if (categoryId == 0)
            {
                return Json(classTypes);
            }

            var typesCategory = await _classTypeRepository.GetTypeFromCategory(categoryId);

            return Json(typesCategory);
        }

        public async Task<JsonResult> GetAvailableInstructorsOnline(DateTime dateStart, DateTime dateEnd, int gymSelect)
        {
            List<Instructor> instructors = new();

            if (dateStart > dateEnd || dateStart == dateEnd || dateStart < DateTime.UtcNow || dateEnd < DateTime.UtcNow)
            {
                return Json(instructors);
            }

            instructors = await _userHelper.GetUsersByTypeAsync<Instructor>();

            List<Instructor> busyInstructors = new();

            // Get all online classes
            List<OnlineClass> onlineClasses = await _classRepository.GetAllOnlineClassesInclude();

            // Iterate through each online class to check for overlaps
            foreach (OnlineClass onlineClass in onlineClasses)
            {
                // Check if the class overlaps with the requested time range
                if (onlineClass.DateStart < dateEnd && onlineClass.DateEnd > dateStart)
                {
                    // The class overlaps remove its instructor from the available list
                    busyInstructors.Add(onlineClass.Instructor);
                }
            }

            List<GymClass> gymClasses = await _classRepository.GetAllGymClassesInclude();

            // Iterate through each gym class to check for overlaps
            foreach (GymClass gymClass in gymClasses)
            {
                // Check if the class overlaps with the requested time range
                if (gymClass.DateStart < dateEnd && gymClass.DateEnd > dateStart)
                {
                    // If the class overlaps remove its instructor from the available list
                    busyInstructors.Add(gymClass.Instructor);
                }
            }

            busyInstructors = busyInstructors.Distinct().ToList();

            instructors.RemoveAll(i => busyInstructors.Any(b => b.Id == i.Id));

            instructors = instructors.Where(i => i.GymId == gymSelect).ToList();

            // Return the filtered list of available instructors
            return Json(instructors);
        }

        public async Task<JsonResult> GetAvailableInstructorsGym(DateTime dateStart, DateTime dateEnd, int gymSelect)
        {
            List<Instructor> instructors = new();

            if (dateStart > dateEnd || dateStart == dateEnd || dateStart < DateTime.UtcNow || dateEnd < DateTime.UtcNow)
            {
                return Json(instructors);
            }

            instructors = await _userHelper.GetUsersByTypeAsync<Instructor>();

            List<Instructor> busyInstructors = new();

            // Get all online classes
            List<OnlineClass> onlineClasses = await _classRepository.GetAllOnlineClassesInclude();

            // Iterate through each online class to check for overlaps
            foreach (OnlineClass onlineClass in onlineClasses)
            {
                // Check if the class overlaps with the requested time range
                if (onlineClass.DateStart < dateEnd && onlineClass.DateEnd > dateStart)
                {
                    // The class overlaps remove its instructor from the available list
                    busyInstructors.Add(onlineClass.Instructor);
                }
            }

            List<GymClass> gymClasses = await _classRepository.GetAllGymClassesInclude();

            // Iterate through each gym class to check for overlaps
            foreach (GymClass gymClass in gymClasses)
            {
                // Check if the class overlaps with the requested time range
                if (gymClass.DateStart < dateEnd && gymClass.DateEnd > dateStart)
                {
                    // If the class overlaps remove its instructor from the available list
                    busyInstructors.Add(gymClass.Instructor);
                }
            }

            busyInstructors = busyInstructors.Distinct().ToList();

            instructors.RemoveAll(i => busyInstructors.Any(b => b.Id == i.Id));

            instructors = instructors.Where(i => i.GymId == gymSelect).ToList();

            // Return the filtered list of available instructors
            return Json(instructors);
        }

        public async Task<JsonResult> GetAvailableInstructorsOnlineEdit(DateTime dateStart, DateTime dateEnd, int gymSelect, int classId)
        {
            List<Instructor> instructors = new();

            if (dateStart > dateEnd || dateStart == dateEnd || dateStart < DateTime.UtcNow || dateEnd < DateTime.UtcNow)
            {
                return Json(instructors);
            }

            instructors = await _userHelper.GetUsersByTypeAsync<Instructor>();

            List<Instructor> busyInstructors = new();

            // Get all online classes
            List<OnlineClass> onlineClasses = await _classRepository.GetAllOnlineClassesInclude();

            // Iterate through each online class to check for overlaps
            foreach (OnlineClass onlineClass in onlineClasses)
            {
                // Check if the class overlaps with the requested time range
                if (!(onlineClass.DateEnd < dateStart || onlineClass.DateStart > dateEnd) && onlineClass.Id != classId)
                {
                    // The class overlaps remove its instructor from the available list
                    busyInstructors.Add(onlineClass.Instructor);
                }
            }

            List<GymClass> gymClasses = await _classRepository.GetAllGymClassesInclude();

            // Iterate through each gym class to check for overlaps
            foreach (GymClass gymClass in gymClasses)
            {
                // Check if the class overlaps with the requested time range
                if (!(gymClass.DateEnd < dateStart || gymClass.DateStart > dateEnd))
                {
                    // If the class overlaps remove its instructor from the available list
                    busyInstructors.Add(gymClass.Instructor);
                }
            }

            instructors = instructors.Where(i => i.GymId == gymSelect).ToList();

            instructors.RemoveAll(i => busyInstructors.Any(b => b.Id == i.Id));

            // Return the filtered list of available instructors
            return Json(instructors);
        }

        public async Task<JsonResult> GetAvailableInstructorsGymEdit(DateTime dateStart, DateTime dateEnd, int classId, int gymSelect)
        {
            List<Instructor> instructors = new();

            if (dateStart > dateEnd || dateStart == dateEnd || dateStart < DateTime.UtcNow || dateEnd < DateTime.UtcNow)
            {
                return Json(instructors);
            }

            instructors = await _userHelper.GetUsersByTypeAsync<Instructor>();

            List<Instructor> busyInstructors = new();

            // Get all online classes
            List<OnlineClass> onlineClasses = await _classRepository.GetAllOnlineClassesInclude();

            // Iterate through each online class to check for overlaps
            foreach (OnlineClass onlineClass in onlineClasses)
            {
                // Check if the class overlaps with the requested time range
                if (!(onlineClass.DateEnd < dateStart || onlineClass.DateStart > dateEnd))
                {
                    // The class overlaps remove its instructor from the available list
                    busyInstructors.Add(onlineClass.Instructor);
                }
            }

            List<GymClass> gymClasses = await _classRepository.GetAllGymClassesInclude();

            // Iterate through each gym class to check for overlaps
            foreach (GymClass gymClass in gymClasses)
            {
                // Check if the class overlaps with the requested time range
                if (!(gymClass.DateEnd < dateStart || gymClass.DateStart > dateEnd) && gymClass.Id != classId)
                {
                    // If the class overlaps remove its instructor from the available list
                    busyInstructors.Add(gymClass.Instructor);
                }
            }

            instructors.RemoveAll(i => busyInstructors.Any(b => b.Id == i.Id));

            instructors = instructors.Where(i => i.GymId == gymSelect).ToList();

            // Return the filtered list of available instructors
            return Json(instructors);
        }

        #endregion

        [Authorize(Roles = "Client, Admin, MasterAdmin, Instructor, Employee")]
        [AllowAnonymous]
        public async Task<IActionResult> Available()
        {
            var categories = await _classCategoryRepository.GetCategoriesSelectListAsync();

            var types = await _classTypeRepository.GetAll().Include(t => t.ClassCategory).ToListAsync();

            ViewBag.Categories = new SelectList(categories, "Value", "Text");

            return View(types);
        }

        [Authorize(Roles = "Client, Admin, MasterAdmin, Instructor, Employee")]
        [AllowAnonymous]
        public async Task<IActionResult> GetClassesByCategory(int? categoryId)
        {
            var types = await _classTypeRepository.GetAll()
                .Include(t => t.ClassCategory)
                .ToListAsync();

            if (categoryId.HasValue && categoryId != 0)
            {
                types = types.Where(t => t.ClassCategory != null && t.ClassCategory.Id == categoryId.Value).ToList();
            }

            return PartialView("_ClassTypesPartial", types);
        }

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
    }
}