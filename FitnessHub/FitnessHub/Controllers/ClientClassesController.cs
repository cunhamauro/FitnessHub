using FitnessHub.Data.Entities.GymClasses;
using FitnessHub.Data.Entities.History;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace FitnessHub.Controllers
{
    public class ClientClassesController : Controller
    {

        private readonly IClassRepository _classRepository;
        private readonly IUserHelper _userHelper;
        private readonly IRegisteredInClassesHistoryRepository _registeredInClassesHistoryRepository;
        private readonly IClassHistoryRepository _classHistoryRepository;
        private readonly IGymHistoryRepository _gymHistoryRepository;
        private readonly IStaffHistoryRepository _staffHistoryRepository;
        private readonly IClassTypeRepository _classTypeRepository;
        private readonly IMembershipDetailsRepository _membershipDetailsRepository;

        public ClientClassesController(IClassRepository classRepository,
                                  IUserHelper userHelper, IRegisteredInClassesHistoryRepository registeredInClassesHistoryRepository, IClassHistoryRepository classHistoryRepository, IGymHistoryRepository gymHistoryRepository, IStaffHistoryRepository staffHistoryRepository, IClassTypeRepository classTypeRepository, IMembershipDetailsRepository membershipDetailsRepository)
        {
            _classRepository = classRepository;
            _userHelper = userHelper;
            _registeredInClassesHistoryRepository = registeredInClassesHistoryRepository;
            _classHistoryRepository = classHistoryRepository;
            _gymHistoryRepository = gymHistoryRepository;
            _staffHistoryRepository = staffHistoryRepository;
            _classTypeRepository = classTypeRepository;
            _membershipDetailsRepository = membershipDetailsRepository;
        }

        public async Task<IActionResult> MyClassHistory()
        {
            var client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
            }

            //var gym = await 

            List<RegisteredInClassesHistory> records = await _registeredInClassesHistoryRepository.GetAll().Where(c => c.UserId == client.Id).ToListAsync();

            List<RegisteredInClassesHistoryViewModel> model = new();

            foreach (var r in records)
            {

                ClassHistory gClass = await _classHistoryRepository.GetByIdAsync(r.ClassId);

                // Fetch employee and instructor if available
                StaffHistory? employee = null;
                StaffHistory? instructor = null;

                var gym = await _gymHistoryRepository.GetByName(gClass.GymName);
                if(gym == null)
                {
                    return GymNotFound();
                }

                if (!string.IsNullOrEmpty(r.EmployeeId))
                {
                    employee = await _staffHistoryRepository.GetByStaffIdAndGymIdTrackAsync(r.EmployeeId, gym.Id);
                }

                if (!string.IsNullOrEmpty(gClass.InstructorId))
                {
                    instructor = await _staffHistoryRepository.GetByStaffIdAndGymIdTrackAsync(gClass.InstructorId, gym.Id);
                }

                model.Add(new RegisteredInClassesHistoryViewModel
                {
                    Id = r.Id,
                    GymName = gClass.GymName,
                    CategoryName = gClass.Category,
                    TypeName = gClass.ClassType,
                    EmployeeEmail = employee?.Email ?? string.Empty,
                    EmployeeFullName = employee != null ? $"{employee.FirstName} {employee.LastName}" : string.Empty,
                    SubClass = gClass.SubClass,
                    RegistrationDate = r.RegistrationDate,
                    InstructorEmail = instructor?.Email ?? string.Empty,
                    InstructorFullName = instructor != null ? $"{instructor.FirstName} {instructor.LastName}" : string.Empty,
                    Reviewed = r.Reviewed,
                    Rating = r.Rating,
                    StartDate = gClass.DateStart.Value,
                    EndDate = gClass.DateEnd.Value,
                });

                ClassType? type = await _classTypeRepository.GetAll().Where(t => t.Name == gClass.ClassType).FirstOrDefaultAsync();

                if (type == null)
                {
                    ViewBag.TypeAvailable = false;
                }
            }

            model = model.Where(r => r.EndDate < DateTime.UtcNow).ToList();

            return View(model);
        }

        // User side actions
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> AvailableClasses()
        {
            var client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
            }

            var memberShipDetailClient = await _membershipDetailsRepository.GetByIdAsync(client.MembershipDetailsId.Value);

            if (client.MembershipDetailsId == null || memberShipDetailClient == null)
            {
                return MembershipNotFound();
            }

            var gymClasses = await _classRepository.GetAllGymClassesInclude();
            gymClasses = gymClasses.Where(c => c.Clients.Count < c.Capacity).ToList();
            var onlineClasses = await _classRepository.GetAllOnlineClassesInclude();

            var viewModel = new AvailableClassesViewModel
            {
                Classes = new List<ClassViewModel>()
            };

            foreach (var gymClass in gymClasses)
            {
                if (!gymClass.Clients.Any(c => c.Id == client.Id))
                {
                    viewModel.Classes.Add(new ClassViewModel
                    {
                        InstructorName = gymClass.Instructor.FullName,
                        DateStart = gymClass.DateStart,
                        DateEnd = gymClass.DateEnd,
                        Location = gymClass.Gym.Name,
                        IsOnline = false,
                        Id = gymClass.Id,
                        Category = gymClass.Category.Name,
                        ClassType = gymClass.ClassType.Name,
                    });
                }
            }

            foreach (var onlineClass in onlineClasses)
            {
                if (!onlineClass.Clients.Any(c => c.Id == client.Id))
                {
                    viewModel.Classes.Add(new ClassViewModel
                    {
                        InstructorName = onlineClass.Instructor.FullName,
                        DateStart = onlineClass.DateStart,
                        DateEnd = onlineClass.DateEnd,
                        Location = "Online",
                        IsOnline = true,
                        Id = onlineClass.Id,
                        Category = onlineClass.Category.Name,
                        ClassType = onlineClass.ClassType.Name,
                    });
                }
            }
            viewModel.Classes = viewModel.Classes.OrderBy(c => c.DateStart).ToList();

            return View(viewModel);
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> Register(int classId, bool isOnline)
        {
            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
            }

            var memberShipDetailClient = await _membershipDetailsRepository.GetByIdAsync(client.MembershipDetailsId.Value);

            if (client.MembershipDetailsId == null || memberShipDetailClient == null)
            {
                return RedirectToAction("Available","Memberships");
            }

            var history = new RegisteredInClassesHistory
            {
                UserId = client.Id,
                ClassId = classId,
                RegistrationDate = DateTime.UtcNow,
                Canceled = false,
            };

            if (isOnline)
            {
                var onlineClass = await _classRepository.GetOnlineClassByIdInclude(classId);

                if (onlineClass == null)
                {
                    return ClassNotFound();
                }

                if (!onlineClass.Clients.Any(c => c.Id == client.Id))
                {
                    onlineClass.Clients.Add(client);
                    await _classRepository.UpdateAsync(onlineClass);
                    await _registeredInClassesHistoryRepository.CreateAsync(history);
                }
            }
            else
            {
                var gymClass = await _classRepository.GetGymClassByIdInclude(classId);

                if (gymClass == null)
                {
                    return ClassNotFound();
                }

                if (gymClass.Clients.Count == gymClass.Capacity)
                {
                    return ClassNotFound();
                }

                if (!gymClass.Clients.Any(c => c.Id == client.Id))
                {
                    gymClass.Clients.Add(client);
                    await _classRepository.UpdateAsync(gymClass);
                    await _registeredInClassesHistoryRepository.CreateAsync(history);
                }
            }
            return RedirectToAction(nameof(AvailableClasses));
        }

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyClasses()
        {
            var client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
            }

            var gymClasses = await _classRepository.GetAllGymClassesInclude();
            var onlineClasses = await _classRepository.GetAllOnlineClassesInclude();

            var viewModel = new AvailableClassesViewModel
            {
                Classes = new List<ClassViewModel>()
            };

            foreach (var gymClass in gymClasses)
            {
                if (gymClass.Clients.Any(c => c.Id == client.Id))
                {
                    viewModel.Classes.Add(new ClassViewModel
                    {
                        InstructorName = gymClass.Instructor.FullName,
                        DateStart = gymClass.DateStart,
                        DateEnd = gymClass.DateEnd,
                        Location = gymClass.Gym.Name,
                        IsOnline = false,
                        Id = gymClass.Id,
                        Category = gymClass.Category.Name,
                        ClassType = gymClass.ClassType.Name,
                    });
                }
            }

            foreach (var onlineClass in onlineClasses)
            {
                if (onlineClass.Clients.Any(c => c.Id == client.Id))
                {
                    viewModel.Classes.Add(new ClassViewModel
                    {
                        InstructorName = onlineClass.Instructor.FullName,
                        DateStart = onlineClass.DateStart,
                        DateEnd = onlineClass.DateEnd,
                        Location = "Online",
                        IsOnline = true,
                        Id = onlineClass.Id,
                        Category = onlineClass.Category.Name,
                        ClassType = onlineClass.ClassType.Name,
                    });
                }
            }
            viewModel.Classes = viewModel.Classes.OrderBy(c => c.DateStart).ToList();
            return View(viewModel);
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> Unregister(int id)
        {
            var client = await _userHelper.GetUserAsync(User) as Client;
            if (client == null)
            {
                return UserNotFound();
            }

            var historyEntry = await _registeredInClassesHistoryRepository.GetHistoryEntryAsync(id, client.Id);

            if (historyEntry != null)
            {
                historyEntry.Canceled = true;
                await _registeredInClassesHistoryRepository.UpdateAsync(historyEntry);
            }

            var gymClass = await _classRepository.GetGymClassByIdIncludeTracked(id);
            if (gymClass != null && gymClass.Clients.Contains(client))
            {
                gymClass.Clients.Remove(client);
                await _classRepository.UpdateAsync(gymClass);
                return RedirectToAction(nameof(MyClasses));
            }

            var onlineClass = await _classRepository.GetOnlineClassByIdInclude(id);
            if (onlineClass != null && onlineClass.Clients.Contains(client))
            {
                onlineClass.Clients.Remove(client);
                await _classRepository.UpdateAsync(onlineClass);
                return RedirectToAction(nameof(MyClasses));
            }
            return View();
        }

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> ClassDetails(int id)
        {
            var gymClass = await _classRepository.GetGymClassByIdInclude(id);

            if (gymClass != null)
            {
                var viewModel = new ClassesDetailsViewModel
                {
                    InstructorFullName = gymClass.Instructor.FullName,
                    InstructorEmail = gymClass.Instructor.Email,
                    InstructorRating = gymClass.Instructor.Rating,
                    InstructorReviews = gymClass.Instructor.NumReviews,
                    DateStart = gymClass.DateStart,
                    DateEnd = gymClass.DateEnd,
                    Location = gymClass.Gym?.Name ?? "N/A",
                    Category = gymClass.Category.Name,
                    GymName = gymClass.Gym?.Name,
                    ClassType = gymClass.ClassType.Name,
                    Rating = gymClass.ClassType.Rating,
                    NumReviews = gymClass.ClassType.NumReviews,
                    GymAddress = $"{gymClass?.Gym?.Address ?? ""}, {gymClass?.Gym?.City ?? ""}, {gymClass?.Gym?.Country ?? ""}"
                };

                ViewBag.GymId = gymClass.Gym.Id;

                return View(viewModel);
            }

            var onlineClass = await _classRepository.GetOnlineClassByIdInclude(id);

            if (onlineClass != null)
            {
                var viewModel = new ClassesDetailsViewModel
                {
                    InstructorFullName = onlineClass.Instructor.FullName,
                    InstructorEmail = onlineClass.Instructor.Email,
                    InstructorRating = onlineClass.Instructor.Rating,
                    InstructorReviews = onlineClass.Instructor.NumReviews,
                    DateStart = onlineClass.DateStart,
                    DateEnd = onlineClass.DateEnd,
                    Location = "Online",
                    Category = onlineClass.Category.Name,
                    Platform = onlineClass.Platform,
                    ClassType = onlineClass.ClassType.Name,
                    Rating = onlineClass.ClassType.Rating,
                    NumReviews = onlineClass.ClassType.NumReviews,
                };

                return View(viewModel);
            }
            return ClassNotFound();
        }

        //Employee side actions

        [Authorize(Roles = "Employee")]
        public IActionResult FindClientByEmail()
        {
            return View();
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> FindClientByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
                var model = new RegisterClientInClassViewModel();
                return View("FindClientByEmail", model);
            }

            var client = await _userHelper.GetUserByEmailAsync(email) as Client;

            if (client == null)
            {
                ModelState.AddModelError(string.Empty, "Client not found.");
                var model = new RegisterClientInClassViewModel { ClientEmail = email };
                return View("FindClientByEmail", model);
            }

            var memberShipDetailClient = await _membershipDetailsRepository.GetByIdAsync(client.MembershipDetailsId.Value);

            if (client.MembershipDetailsId == null || memberShipDetailClient == null)
            {
                ModelState.AddModelError(string.Empty, "Client does not have a membership");
                var model = new RegisterClientInClassViewModel { ClientEmail = email };
                return View("FindClientByEmail", model);
            }
            return RedirectToAction(nameof(RegisterClientInClass), new { email });
        }

        public async Task<IActionResult> RegisterClientInClass(string email)
        {
            var client = await _userHelper.GetUserByEmailAsync(email) as Client;
            if (client == null)
            {
                return RedirectToAction("FindClientByEmail");
            }

            var employee = await _userHelper.GetUserAsync(this.User) as Employee;

            var classes = await _classRepository.GetAllGymClassesInclude();

            classes = classes.Where(c => c.Gym.Id == employee.GymId && c.Clients.Count < c.Capacity).ToList();

            var model = new RegisterClientInClassViewModel
            {
                ClientEmail = email,
                IsEmailValid = true,
                Classes = classes.Select(c => new ClassDetailsViewModel
                {
                    Id = c.Id,
                    Category = c.Category.Name,
                    ClassType = c.ClassType.Name,
                    InstructorName = c.Instructor.FullName,
                    DateStart = c.DateStart,
                    DateEnd = c.DateEnd,
                    Location = c.Gym.Name,
                    IsClientRegistered = c.Clients.Any(cl => cl.Id == client.Id)
                }).ToList()
            };

            return View("RegisterClientInClass", model);
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> RegisterClientInClassConfirm(RegisterClientInClassViewModel model)
        {
            var clientEmail = model.ClientEmail;

            var client = await _userHelper.GetUserByEmailAsync(model.ClientEmail) as Client;
            if (client == null)
            {
                ModelState.AddModelError(string.Empty, "Client not found.");
                model.Classes = await LoadClassDetails();
                return View("RegisterClientInClass", model);
            }

            var memberShipDetailClient = await _membershipDetailsRepository.GetByIdAsync(client.MembershipDetailsId.Value);

            if (client.MembershipDetailsId == null || memberShipDetailClient == null)
            {
                ModelState.AddModelError(string.Empty, "Client does not have a membership.");
                model.Classes = await LoadClassDetails();
                return View("RegisterClientInClass", model);
            }

            var employee = await _userHelper.GetUserAsync(this.User);

            var allClasses = await _classRepository.GetAllGymClassesInclude();
            allClasses = allClasses.Where(c => c.Clients.Count < c.Capacity).ToList();

            foreach (var gymClass in allClasses)
            {
                var history = new RegisteredInClassesHistory
                {
                    UserId = client.Id,
                    ClassId = gymClass.Id,
                    EmployeeId = employee.Id,
                    RegistrationDate = DateTime.UtcNow,
                    Canceled = false,
                };
                if (model.SelectedClassIds.Contains(gymClass.Id))
                {
                    if (!gymClass.Clients.Any(c => c.Id == client.Id))
                    {
                        gymClass.Clients.Add(client);
                        await _registeredInClassesHistoryRepository.CreateAsync(history);
                    }
                }
                else
                {
                    if (gymClass.Clients.Any(c => c.Id == client.Id))
                    {
                        gymClass.Clients.Remove(client);

                        var historyEntry = await _registeredInClassesHistoryRepository.GetHistoryEntryAsync(gymClass.Id, client.Id);

                        if (historyEntry != null)
                        {
                            historyEntry.Canceled = true;
                            await _registeredInClassesHistoryRepository.UpdateAsync(historyEntry);
                        }
                    }
                }
                await _classRepository.UpdateAsync(gymClass);
            }
            return RedirectToAction("Clients", "Account");
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> ReviewClassAndInstructor(int histId, int rating)
        {
            var clientClassHistory = await _registeredInClassesHistoryRepository.GetByIdAsync(histId);

            if (clientClassHistory == null)
            {
                return RedirectToAction(nameof(MyClassHistory));
            }

            Client? client = await _userHelper.GetClientIncludeAsync(clientClassHistory.UserId);

            if (client == null)
            {
                return RedirectToAction(nameof(MyClassHistory));
            }

            var classHistory = await _classHistoryRepository.GetByIdAsync(clientClassHistory.ClassId);

            if (classHistory == null)
            {
                return RedirectToAction(nameof(MyClassHistory));
            }

            ClassType? type = await _classTypeRepository.GetAll().Where(t => t.Name == classHistory.ClassType).FirstOrDefaultAsync();

            if (type == null)
            {
                return RedirectToAction(nameof(MyClassHistory));
            }

            Instructor? instructor = await _userHelper.GetUserByIdAsync(classHistory.InstructorId) as Instructor;

            if (instructor == null)
            {
                return RedirectToAction(nameof(MyClassHistory));
            }

            type.NumReviews++;
            type.Rating = ((type.Rating * (type.NumReviews - 1)) + rating) / type.NumReviews;

            await _classTypeRepository.UpdateAsync(type);

            instructor.NumReviews++;
            instructor.Rating = ((instructor.Rating * (instructor.NumReviews - 1)) + rating) / instructor.NumReviews;

            await _userHelper.UpdateUserAsync(instructor);

            clientClassHistory.Reviewed = true;
            clientClassHistory.Rating = rating;

            await _registeredInClassesHistoryRepository.UpdateAsync(clientClassHistory);

            return RedirectToAction(nameof(MyClassHistory));
        }

        private async Task<List<ClassDetailsViewModel>> LoadClassDetails()
        {
            var classes = await _classRepository.GetAllGymClassesInclude();
            return classes.Select(c => new ClassDetailsViewModel
            {
                Id = c.Id,
                Category = c.Category.Name,
                InstructorName = c.Instructor.FullName,
                DateStart = c.DateStart,
                DateEnd = c.DateEnd,
                Location = c.Gym.Name,
            }).ToList();
        }

        public IActionResult ClassNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Class not found", Message = "With so many available, how could you not find one?" });
        }

        public IActionResult MembershipNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Membership not found", Message = "Maybe its time to add another membership?" });
        }

        public IActionResult UserNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "User not found", Message = "Looks like this user skipped leg day!" });
        }

        public IActionResult GymNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Gym not found", Message = "With so many worldwide, how did you miss this one?" });
        }
    }
}


