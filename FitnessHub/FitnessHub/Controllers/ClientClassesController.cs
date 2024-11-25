using FitnessHub.Data.Classes;
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
        private readonly IClassWaitlistRepository _classWaitlistRepository;
        private readonly IMailHelper _mailHelper;
        private readonly IGymRepository _gymRepository;
        private readonly IClassCategoryRepository _categoryRepository;

        public ClientClassesController(IClassRepository classRepository,
                                  IUserHelper userHelper,
                                  IRegisteredInClassesHistoryRepository registeredInClassesHistoryRepository,
                                  IClassHistoryRepository classHistoryRepository,
                                  IGymHistoryRepository gymHistoryRepository,
                                  IStaffHistoryRepository staffHistoryRepository,
                                  IClassTypeRepository classTypeRepository,
                                  IMembershipDetailsRepository membershipDetailsRepository,
                                  IClassWaitlistRepository classWaitlistRepository,
                                  IMailHelper mailHelper,
                                  IGymRepository gymRepository,
                                  IClassCategoryRepository categoryRepository)
        {
            _classRepository = classRepository;
            _userHelper = userHelper;
            _registeredInClassesHistoryRepository = registeredInClassesHistoryRepository;
            _classHistoryRepository = classHistoryRepository;
            _gymHistoryRepository = gymHistoryRepository;
            _staffHistoryRepository = staffHistoryRepository;
            _classTypeRepository = classTypeRepository;
            _membershipDetailsRepository = membershipDetailsRepository;
            _classWaitlistRepository = classWaitlistRepository;
            _mailHelper = mailHelper;
            _gymRepository = gymRepository;
            _categoryRepository = categoryRepository;
        }

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyClassHistory()
        {
            var client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
            }

            List<RegisteredInClassesHistory> records = await _registeredInClassesHistoryRepository.GetAll().Where(c => c.UserId == client.Id).ToListAsync();

            List<RegisteredInClassesHistoryViewModel> model = new();

            foreach (var r in records)
            {

                ClassHistory gClass = await _classHistoryRepository.GetByIdAsync(r.ClassId);

                // Fetch employee and instructor if available
                StaffHistory? employee = null;
                StaffHistory? instructor = null;

                GymHistory gym = null;
                if (gClass.GymId.HasValue)
                {
                    gym = await _gymHistoryRepository.GetByIdAsync(gClass.GymId.Value);
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

        [HttpPost]
        public async Task<IActionResult> JoinWaitlist(int classId)
        {
            if (!this.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (client.MembershipDetailsId == null)
            {
                return RedirectToAction("SignUp", "Memberships");
            }

            var memberShipDetailClient = await _membershipDetailsRepository.GetByIdAsync(client.MembershipDetailsId.Value);

            if (memberShipDetailClient == null)
            {
                return RedirectToAction("SignUp", "Memberships");
            }

            if (memberShipDetailClient.Status == false)
            {
                return RedirectToAction("MyMembership", "Memberships");
            }

            var gymClass = await _classRepository.GetGymClassByIdInclude(classId);

            if (gymClass == null)
            {
                return ClassNotFound();
            }

            if (gymClass.Capacity < gymClass.Clients.Count)
            {
                return ClassNotFull();
            }

            if (gymClass.Clients.Contains(client))
            {
                return ClassNotFound();
            }

            var waitlist = await _classWaitlistRepository.GetByIdAsync(classId);

            if (waitlist == null)
            {
                return ClassWaitlistNotFound();
            }

            int highestNumber = waitlist.ClientEmailsOrderedList.Any()
                ? waitlist.ClientEmailsOrderedList
                    .Max(email => int.Parse(email.Split('@')[0])) // Extract number and find max
                : 0; // Default to 0 if the list is empty

            // Increment the highest number for the new email
            int number = highestNumber + 1;

            waitlist.ClientEmailsOrderedList.Add($"{number}@{client.Email}");

            await _classWaitlistRepository.UpdateAsync(waitlist);

            return RedirectToAction(nameof(AvailableClasses));
        }

        [HttpPost]
        public async Task<IActionResult> ExitWaitlist(int classId)
        {
            if (!this.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (client.MembershipDetailsId == null)
            {
                return RedirectToAction("SignUp", "Memberships");
            }

            var memberShipDetailClient = await _membershipDetailsRepository.GetByIdAsync(client.MembershipDetailsId.Value);

            if (memberShipDetailClient == null)
            {
                return RedirectToAction("SignUp", "Memberships");
            }

            if (memberShipDetailClient.Status == false)
            {
                return RedirectToAction("MyMembership", "Memberships");
            }

            var gymClass = await _classRepository.GetGymClassByIdInclude(classId);

            if (gymClass == null)
            {
                return ClassNotFound();
            }

            if (gymClass.Clients.Contains(client))
            {
                return ClassNotFound();
            }

            var waitlist = await _classWaitlistRepository.GetByIdAsync(classId);

            if (waitlist == null)
            {
                return ClassWaitlistNotFound();
            }

            //if (waitlist.GetClientEmails().Contains(client.Email))
            //{
            //    return ClassWaitlistNotFound();
            //}

            string emailToRemove = "";

            foreach (var email in waitlist.ClientEmailsOrderedList)
            {
                if (email.Contains(client.Email))
                {
                    emailToRemove = email;
                    break;
                }
            }

            waitlist.ClientEmailsOrderedList.Remove(emailToRemove);

            await _classWaitlistRepository.UpdateAsync(waitlist);

            return RedirectToAction(nameof(AvailableClasses));
        }

        public async Task<IActionResult> AvailableClasses(AvailableClassesViewModel model, int LocationId = 0, int CategoryId = 0)
        {
            var client = await _userHelper.GetUserAsync(this.User) as Client;

            //if (client == null)
            //{
            //    return UserNotFound();
            //}

            //if (client.MembershipDetailsId == null)
            //{
            //    return MembershipNotFound();
            //}

            //var memberShipDetailClient = await _membershipDetailsRepository.GetByIdAsync(client.MembershipDetailsId.Value);

            //if (memberShipDetailClient == null)
            //{
            //    return MembershipNotFound();
            //}

            //if (memberShipDetailClient.Status == false)
            //{
            //    return MembershipNotFound();
            //}

            model.Categories = _categoryRepository.GetAll()
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();

            model.LocationList = _gymRepository.GetAll()
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                })
                .ToList();

            model.LocationList.Insert(0, new SelectListItem { Value = "-1", Text = "Online Classes" });

            var availableClasses = new List<ClassViewModel>();

            var onlineClasses = await _classRepository.GetAllOnlineClassesInclude();

            onlineClasses = onlineClasses.Where(o => o.DateStart > DateTime.UtcNow).ToList();

            var gymClasses = await _classRepository.GetAllGymClassesInclude();

            gymClasses = gymClasses.Where(o => o.DateStart > DateTime.UtcNow).ToList();

            if (LocationId > 0)
            {
                gymClasses = gymClasses.Where(c => c.Gym.Id == LocationId).ToList();
                onlineClasses = new List<OnlineClass>();
            }

            if (LocationId == -1)
            {
                gymClasses = new List<GymClass>();
            }

            if (model.DateFilter != null)
            {
                onlineClasses = onlineClasses.Where(c => c.DateStart.Date == model.DateFilter.Value.Date).ToList();
                gymClasses = gymClasses.Where(c => c.DateStart.Date == model.DateFilter.Value.Date).ToList();
            }
            if (CategoryId > 0)
            {
                onlineClasses = onlineClasses.Where(c => c.Category.Id == CategoryId).ToList();
                gymClasses = gymClasses.Where(c => c.Category.Id == CategoryId).ToList();
            }

            foreach (var onlineClass in onlineClasses)
            {
                if (client != null && !onlineClass.Clients.Any(c => c.Id == client.Id))
                {
                    availableClasses.Add(new ClassViewModel
                    {
                        InstructorName = onlineClass.Instructor.FullName,
                        DateStart = onlineClass.DateStart,
                        DateEnd = onlineClass.DateEnd,
                        Location = "Online",
                        IsOnline = true,
                        Id = onlineClass.Id,
                        Category = onlineClass.Category.Name,
                        ClassType = onlineClass.ClassType.Name,
                        Full = false,
                        InWaitlist = false,
                        Registered = false,
                    });
                }
                else if (client == null)
                {
                    availableClasses.Add(new ClassViewModel
                    {
                        InstructorName = onlineClass.Instructor.FullName,
                        DateStart = onlineClass.DateStart,
                        DateEnd = onlineClass.DateEnd,
                        Location = "Online",
                        IsOnline = true,
                        Id = onlineClass.Id,
                        Category = onlineClass.Category.Name,
                        ClassType = onlineClass.ClassType.Name,
                        Full = false,
                        InWaitlist = false,
                        Registered = false,
                    });
                }
            }
            foreach (var gymClass in gymClasses)
            {
                if (client != null && !gymClass.Clients.Any(c => c.Id == client.Id))
                {
                    availableClasses.Add(new ClassViewModel
                    {
                        InstructorName = gymClass.Instructor.FullName,
                        DateStart = gymClass.DateStart,
                        DateEnd = gymClass.DateEnd,
                        Location = gymClass.Gym.Name,
                        IsOnline = false,
                        Id = gymClass.Id,
                        Category = gymClass.Category.Name,
                        ClassType = gymClass.ClassType.Name,
                        Full = gymClass.Capacity == gymClass.Clients.Count(),
                        InWaitlist = false,
                        Registered = false,
                    });
                }
                else if (client == null)
                {
                    availableClasses.Add(new ClassViewModel
                    {
                        InstructorName = gymClass.Instructor.FullName,
                        DateStart = gymClass.DateStart,
                        DateEnd = gymClass.DateEnd,
                        Location = gymClass.Gym.Name,
                        IsOnline = false,
                        Id = gymClass.Id,
                        Category = gymClass.Category.Name,
                        ClassType = gymClass.ClassType.Name,
                        Full = gymClass.Capacity == gymClass.Clients.Count(),
                        InWaitlist = false,
                        Registered = false,
                    });
                }
            }

            model.Classes = availableClasses.OrderBy(c => c.DateStart).ToList();
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Register(int classId, bool isOnline)
        {
            if (!this.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (client.MembershipDetailsId == null)
            {
                return RedirectToAction("SignUp", "Memberships");
            }

            var memberShipDetailClient = await _membershipDetailsRepository.GetByIdAsync(client.MembershipDetailsId.Value);

            if (memberShipDetailClient == null)
            {
                return RedirectToAction("SignUp", "Memberships");
            }

            if (memberShipDetailClient.Status == false)
            {
                return RedirectToAction("MyMembership", "Memberships");
            }

            var history = new RegisteredInClassesHistory
            {
                UserId = client.Id,
                ClassId = classId,
                RegistrationDate = DateTime.UtcNow,
                Canceled = false,
            };

            string classesUrl = Url.Action("AvailableClasses", "Classes");
            string body = "";
            string title = "";

            if (isOnline)
            {
                var onlineClass = await _classRepository.GetOnlineClassByIdIncludeTracked(classId);

                if (onlineClass == null || onlineClass.DateStart > DateTime.UtcNow)
                {
                    return ClassNotFound();
                }

                if (!onlineClass.Clients.Any(c => c.Id == client.Id))
                {
                    onlineClass.Clients.Add(client);
                    await _classRepository.UpdateAsync(onlineClass);
                    await _registeredInClassesHistoryRepository.CreateAsync(history);

                    body = _mailHelper.GetEmailTemplate($"Registered in {onlineClass.ClassType.Name} Online Class", @$"Hey, {client.FirstName}, you registered yourself in the <span style=""font-weight: bold"">{onlineClass.ClassType.Name} online class</span>, scheduled to start at <span style=""font-weight: bold"">{onlineClass.DateStart.ToLongDateString()}</span> and finishing at <span style=""font-weight: bold"">{onlineClass.DateEnd.ToLongDateString()}</span> on <span style=""font-weight: bold"">{onlineClass.Platform}</span>.", "Check our other available classes");
                    title = "Online class registration";
                }
            }
            else
            {
                var gymClass = await _classRepository.GetGymClassByIdIncludeTracked(classId);

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

                    body = _mailHelper.GetEmailTemplate($"Registered in {gymClass.ClassType.Name} Class", @$"Hey, {client.FirstName}, you registered yourself in the <span style=""font-weight: bold"">{gymClass.ClassType.Name} class</span>, scheduled to start at <span style=""font-weight: bold"">{gymClass.DateStart.ToLongDateString()}</span> and finishing at <span style=""font-weight: bold"">{gymClass.DateEnd.ToLongDateString()}</span> at <span style=""font-weight: bold"">{gymClass.Gym.Data}</span>.", @$"Check our other <a href=""{classesUrl}"">available classes</a>");
                    title = "Online class registration";

                    var waitlist = await _classWaitlistRepository.GetByIdAsync(gymClass.Id);

                    string emailToRemoveWaiting = "";

                    foreach (var inWait in waitlist.ClientEmailsOrderedList)
                    {
                        if (inWait.Contains(client.Email))
                        {
                            emailToRemoveWaiting = client.Email;
                        }
                    }

                    waitlist.ClientEmailsOrderedList.Remove(emailToRemoveWaiting);
                }
            }

            Response response2 = await _mailHelper.SendEmailAsync(client.Email, $"{title}", body, null, null);

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
            gymClasses = gymClasses.OrderBy(c => c.DateStart).ToList();

            var onlineClasses = await _classRepository.GetAllOnlineClassesInclude();
            onlineClasses = onlineClasses.OrderBy(o => o.DateStart).ToList();

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
            viewModel.Classes = viewModel.Classes.Where(c => c.DateStart >= DateTime.UtcNow).OrderBy(c => c.DateStart).ToList();
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

            string body = "";
            string title = "";

            string classesUrl = Url.Action("AvailableClasses", "Classes");

            var gymClass = await _classRepository.GetGymClassByIdIncludeTracked(id);
            if (gymClass != null && gymClass.Clients.Contains(client))
            {
                gymClass.Clients.Remove(client);
                await _classRepository.UpdateAsync(gymClass);

                body = _mailHelper.GetEmailTemplate($"Unregistered From {gymClass.ClassType.Name} Class", @$"Hey, {client.FirstName}, you unregistered yourself from a <span style=""font-weight: bold"">{gymClass.ClassType.Name} class</span>, scheduled to start at <span style=""font-weight: bold"">{gymClass.DateStart.ToLongDateString()}</span> and finishing at <span style=""font-weight: bold"">{gymClass.DateEnd.ToLongDateString()}</span> at <span style=""font-weight: bold"">{gymClass.Gym.Data}</span>", "Check our other available classes");
                title = "Unregistered from class";

                // Waiting list automation

                var waitlist = await _classWaitlistRepository.GetByIdAsync(gymClass.Id);

                if (waitlist.ClientEmailsOrderedList.Any())
                {
                    string waitingClientEmail = await WaitingListAutomation(gymClass, waitlist);

                    string body2 = _mailHelper.GetEmailTemplate($"Registered in {gymClass.ClassType.Name} Class", @$"Hey, {client.FirstName}, you were automatically registered in the <span style=""font-weight: bold"">{gymClass.ClassType.Name} class</span>, scheduled to start at <span style=""font-weight: bold"">{gymClass.DateStart.ToLongDateString()}</span> and finishing at <span style=""font-weight: bold"">{gymClass.DateEnd.ToLongDateString()}</span> at <span style=""font-weight: bold"">{gymClass.Gym.Data}</span>, as you were the first in the waiting list.", @$"Check our other <a href=""{classesUrl}"">available classes</a>");
                    string title2 = "Automatic class registration";
                    Response response2 = await _mailHelper.SendEmailAsync(waitingClientEmail, $"{title2}", body2, null, null);
                }

                //

            }

            var onlineClass = await _classRepository.GetOnlineClassByIdInclude(id);
            if (onlineClass != null && onlineClass.Clients.Contains(client))
            {
                onlineClass.Clients.Remove(client);
                await _classRepository.UpdateAsync(onlineClass);

                body = _mailHelper.GetEmailTemplate($"Unregistered from {onlineClass.ClassType.Name} Online Class", @$"Hey, {client.FirstName}, you unregistered yourself from a <span style=""font-weight: bold"">{onlineClass.ClassType.Name} online class</span>, scheduled to start at <span style=""font-weight: bold"">{onlineClass.DateStart.ToLongDateString()}</span> and finishing at <span style=""font-weight: bold"">{onlineClass.DateEnd.ToLongDateString()}</span> on <span style=""font-weight: bold"">{onlineClass.Platform}</span>", @$"Check our other <a href=""{classesUrl}"">available classes</a>");
                title = "Unregistered from online class";

            }

            Response response = await _mailHelper.SendEmailAsync(client.Email, $"{title}", body, null, null);

            return RedirectToAction(nameof(MyClasses));
        }

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

        [HttpPost]
        public async Task<IActionResult> FindClientByEmail(string email)
        {
            var employee = await _userHelper.GetUserAsync(this.User) as Employee;

            if (employee == null)
            {
                return UserNotFound();
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Email is required");
                var model = new RegisterClientInClassViewModel();
                return View("FindClientByEmail", model);
            }

            var client = await _userHelper.GetUserByEmailAsync(email) as Client;

            if (client == null)
            {
                ModelState.AddModelError(string.Empty, "Client not found");
                var model = new RegisterClientInClassViewModel { ClientEmail = email };
                return View("FindClientByEmail", model);
            }

            if (employee.GymId != client.GymId)
            {
                ModelState.AddModelError(string.Empty, "This client is not assigned to your Gym");
                var model = new RegisterClientInClassViewModel { ClientEmail = email };
                return View("FindClientByEmail", model);
            }

            if (client.MembershipDetailsId == null)
            {
                ModelState.AddModelError(string.Empty, "Client does not have a membership");
                var model = new RegisterClientInClassViewModel { ClientEmail = email };
                return View("FindClientByEmail", model);
            }

            var memberShipDetailClient = await _membershipDetailsRepository.GetByIdAsync(client.MembershipDetailsId.Value);

            if (memberShipDetailClient == null)
            {
                ModelState.AddModelError(string.Empty, "Client does not have a membership");
                var model = new RegisterClientInClassViewModel { ClientEmail = email };
                return View("FindClientByEmail", model);
            }

            if (memberShipDetailClient.Status == false)
            {
                return MembershipNotFound();
            }

            return RedirectToAction(nameof(RegisterClientInClass), new { email });
        }

        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> RegisterClientInClass(string email)
        {
            var client = await _userHelper.GetUserByEmailAsync(email) as Client;

            if (client == null)
            {
                return RedirectToAction("FindClientByEmail");
            }

            var employee = await _userHelper.GetUserAsync(this.User) as Employee;

            var classes = await _classRepository.GetAllGymClassesInclude();

            classes = classes.Where(c => c.Gym.Id == employee.GymId && (c.Clients.Count < c.Capacity || c.Clients.Contains(client))).OrderBy(c => c.DateStart).ToList();

            var model = new RegisterClientInClassViewModel
            {
                ClientEmail = email,
                ClientFullName = client.FullName,
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

        [HttpPost]
        public async Task<IActionResult> RegisterClientInClassConfirm(RegisterClientInClassViewModel model)
        {
            var clientEmail = model.ClientEmail;
            string classesUrl = Url.Action("AvailableClasses", "Classes");

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

            if (memberShipDetailClient.Status == false)
            {
                return MembershipNotFound();
            }

            var employee = await _userHelper.GetUserAsync(this.User);

            var allClasses = await _classRepository.GetAllGymClassesInclude();
            //allClasses = allClasses.Where(c => c.Clients.Count < c.Capacity).ToList();

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

                        string body = _mailHelper.GetEmailTemplate($"Registered in {gymClass.ClassType.Name} Class", @$"Hey, {client.FirstName}, you were registered by our employee <span style=""font-weight: bold"">{employee.FullName}</span> at <span style=""font-weight: bold"">{gymClass.Gym.Data}</span>!"" in the <span style=""font-weight: bold"">{gymClass.ClassType.Name} class</span>, scheduled to start at <span style=""font-weight: bold"">{gymClass.DateStart.ToLongDateString()}</span> and finishing at <span style=""font-weight: bold"">{gymClass.DateEnd.ToLongDateString()}</span> at <span style=""font-weight: bold"">{gymClass.Gym.Data}</span>.", "Check our other available classes");
                        string title = "Class registration";
                        Response response = await _mailHelper.SendEmailAsync(client.Email, $"{title}", body, null, null);

                        var waitlist = await _classWaitlistRepository.GetByIdAsync(gymClass.Id);

                        string emailToRemoveWaiting = "";

                        foreach (var inWait in waitlist.ClientEmailsOrderedList)
                        {
                            if (inWait.Contains(client.Email))
                            {
                                emailToRemoveWaiting = client.Email;
                            }
                        }

                        waitlist.ClientEmailsOrderedList.Remove(emailToRemoveWaiting);
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

                        string body = _mailHelper.GetEmailTemplate($"Unregistered From {gymClass.ClassType.Name} Class", @$"Hey, {client.FirstName}, you were unregistered by our employee <span style=""font-weight: bold"">{employee.FullName}</span> at <span style=""font-weight: bold"">{gymClass.Gym.Data}</span>!"" from the <span style=""font-weight: bold"">{gymClass.ClassType.Name} class</span>, scheduled to start at <span style=""font-weight: bold"">{gymClass.DateStart.ToLongDateString()}</span> and finishing at <span style=""font-weight: bold"">{gymClass.DateEnd.ToLongDateString()}</span> at <span style=""font-weight: bold"">{gymClass.Gym.Data}</span>.", @$"Check our other <a href=""{classesUrl}"">available classes</a>");
                        string title = "Unregistered from class";
                        Response response = await _mailHelper.SendEmailAsync(client.Email, $"{title}", body, null, null);

                        //

                        // Waiting list automation

                        var waitlist = await _classWaitlistRepository.GetByIdAsync(gymClass.Id);

                        if (waitlist.ClientEmailsOrderedList.Any())
                        {
                            string waitingClientEmail = await WaitingListAutomation(gymClass, waitlist);

                            string body2 = _mailHelper.GetEmailTemplate($"Registered in {gymClass.ClassType.Name} Class", @$"Hey, {client.FirstName}, you were automatically registered in the <span style=""font-weight: bold"">{gymClass.ClassType.Name} class</span>, scheduled to start at <span style=""font-weight: bold"">{gymClass.DateStart.ToLongDateString()}</span> and finishing at <span style=""font-weight: bold"">{gymClass.DateEnd.ToLongDateString()}</span> at <span style=""font-weight: bold"">{gymClass.Gym.Data}</span>, as you were the first in the waiting list.", @$"Check our other <a href=""{classesUrl}"">available classes</a>");
                            string title2 = "Automatic class registration";
                            Response response2 = await _mailHelper.SendEmailAsync(waitingClientEmail, $"{title2}", body2, null, null);
                        }

                        //
                    }
                }
                await _classRepository.UpdateAsync(gymClass);
            }
            return RedirectToAction("RegisterClientInClass", "ClientClasses", new { email = model.ClientEmail });
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

            return RedirectToAction("ClientHistory", "Account");
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

        public IActionResult ClassWaitlistNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Class waitlist not found", Message = "No waitlist here for you! Next time try to register early!" });
        }

        public IActionResult ClassNotFull()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Class not full", Message = "Are you trying to take up two spaces?" });
        }

        public IActionResult MembershipNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Membership not found", Message = "Maybe its time for another one?" });
        }

        public IActionResult UserNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "User not found", Message = "Looks like this user skipped leg day!" });
        }

        public IActionResult GymNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Gym not found", Message = "With so many worldwide, how did you miss this one?" });
        }

        public async Task<string> WaitingListAutomation(GymClass gymClass, ClassWaitlist waitlist)
        {
            // Get the list of emails sorted by the number before '@'
            var sortedEmails = waitlist.ClientEmailsOrderedList
                .OrderBy(email => int.Parse(email.Split('@')[0])) // Extract number before '@' and sort by it
                .ToList();

            var firstEmail = sortedEmails.FirstOrDefault();

            // Split the string at the first "@" only (2 parts: before and after the first @)
            string emailToRegister = firstEmail.Split('@', 2)[1];

            // Automatic client in class registration

            Client waitingClient = await _userHelper.GetUserByEmailAsync(emailToRegister) as Client;

            var history2 = new RegisteredInClassesHistory
            {
                UserId = waitingClient.Id,
                ClassId = gymClass.Id,
                RegistrationDate = DateTime.UtcNow,
                Canceled = false,
            };

            if (!gymClass.Clients.Any(c => c.Id == waitingClient.Id))
            {
                gymClass.Clients.Add(waitingClient);
                await _registeredInClassesHistoryRepository.CreateAsync(history2);

                waitlist.ClientEmailsOrderedList.Remove(firstEmail);
                await _classWaitlistRepository.UpdateAsync(waitlist);
            }

            await _classRepository.UpdateAsync(gymClass);

            return waitingClient.Email;
        }
    }
}