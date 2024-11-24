using FitnessHub.Data.Classes;
using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.History;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using FitnessHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessHub.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly ILoadHelper _loadHelper;
        private readonly IGymRepository _gymRepository;
        private readonly IClientHistoryRepository _clientHistoryRepository;
        private readonly IStaffHistoryRepository _staffHistoryRepository;
        private readonly IGymHistoryRepository _gymHistoryRepository;
        private readonly CountryService _countryService;
        private readonly IClassRepository _classRepository;
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IMembershipRepository _membershipRepository;

        public UsersController(
            IUserHelper userHelper,
            IMailHelper mailHelper,
            ILoadHelper loadHelper,
            IGymRepository gymRepository,
            IClientHistoryRepository clientHistoryRepository,
            IStaffHistoryRepository staffHistoryRepository,
            IMembershipRepository membershipRepository,
            IGymHistoryRepository gymHistoryRepository,
            CountryService countryService,
            IClassRepository classRepository,
            IWorkoutRepository workoutRepository)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _loadHelper = loadHelper;
            _gymRepository = gymRepository;
            _clientHistoryRepository = clientHistoryRepository;
            _staffHistoryRepository = staffHistoryRepository;
            _gymHistoryRepository = gymHistoryRepository;
            _countryService = countryService;
            _classRepository = classRepository;
            _workoutRepository = workoutRepository;
            _membershipRepository = membershipRepository;
        }

        // GET: Users
        [Authorize(Roles = "MasterAdmin, Admin")]
        public async Task<IActionResult> Index()
        {
            var users = new List<User>();

            if (this.User.IsInRole("MasterAdmin"))
            {
                users = (await _userHelper.GetAdminsAsync()).ToList();
            }

            if (this.User.IsInRole("Admin"))
            {
                var admin = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                if (admin == null)
                {
                    return UserNotFound();
                }

                var adminGym = await _gymRepository.GetGymByUserAsync(admin);
                if (adminGym == null)
                {
                    return GymNotFound();
                }

                users = (await _userHelper.GetEmployeesAndInstructorsAndClientsByGymAsync(adminGym.Id)).ToList();
            }

            var usersList = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var role = (await _userHelper.GetUserRolesAsync(user)).FirstOrDefault();
                Gym? gym = await _gymRepository.GetGymByUserAsync(user);

                usersList.Add(new UserRoleViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = role,
                    Gym = gym,
                    Avatar = user.Avatar,
                    
                });
            }

            return View(usersList);
        }

        // GET: Users/Details/5
        [Authorize(Roles = "MasterAdmin, Admin, Instructor")]
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return UserNotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(id);
            if (user == null)
            {
                return UserNotFound();
            }

            var userGym = await _gymRepository.GetGymByUserAsync(user);
            if (userGym == null)
            {
                return GymNotFound();
            }

            var roles = await _userHelper.GetUserRolesAsync(user);
            var role = roles.FirstOrDefault();

            var model = new UserDetailsViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Gym = userGym,
                Role = role,
                BirthDate = user.BirthDate,
                PhoneNumber = user.PhoneNumber,
                Avatar = user.Avatar,
            };

            //var client = await _userHelper.GetClientIncludeAsync(id);

            //bool disabled = false;

            //if (client.MembershipDetails != null)
            //{
            //    disabled = true;
            //}

            //if ((client.OnlineClass != null && client.OnlineClass.Any()) ||
            //    (client.GymClass != null && client.GymClass.Any()))
            //{
            //    disabled = true;
            //}
            //if (disabled)
            //{
            //    ViewBag.Status = "disabled";
            //}

            return View(model);
        }

        // GET: Users/Create
        [Authorize(Roles = "MasterAdmin, Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new AdminRegisterNewUserViewModel();

            var countries = await _loadHelper.LoadCountriesAsync();
            model.Countries = new SelectList(countries, "Callingcode", "Data");

            if (this.User.IsInRole("MasterAdmin"))
            {
                model.Gyms = _gymRepository.GetAll().Select(gym => new SelectListItem
                {
                    Value = gym.Id.ToString(),
                    Text = $"{gym.Data}",
                });

                _loadHelper.LoadMasterAdminRoles(model);
            }

            if (this.User.IsInRole("Admin"))
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                if (user == null)
                {
                    return UserNotFound();
                }

                var gym = await _gymRepository.GetGymByUserAsync(user);
                if (gym == null)
                {
                    return GymNotFound();
                }

                model.Gyms = new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Value = gym.Id.ToString(),
                        Text = $"{gym.Data}"
                    }
                };

                _loadHelper.LoadAdminRoles(model);
            }

            return View(model);
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminRegisterNewUserViewModel model)
        {
            if (model.Gym < 1)
            {
                ModelState.AddModelError("Gym", "Please select a gym.");
            }

            if (model.Role == "0")
            {
                ModelState.AddModelError("Role", "Please select a role.");
            }

            if (model.CountryCallingcode == null || model.CountryCallingcode == "undefined")
            {
                ModelState.AddModelError("PhoneNumber", "Please select a country.");
            }

            if (this.User.IsInRole("Admin"))
            {
                ModelState.Remove("Gym");
            }

            if (this.User.IsInRole("MasterAdmin"))
            {
                ModelState.Remove("Role");
            }

            model.Gyms = _gymRepository.GetAll().Select(gym => new SelectListItem
            {
                Value = gym.Id.ToString(),
                Text = $"{gym.Data}",
            });

            var countries = await _loadHelper.LoadCountriesAsync();
            model.Countries = new SelectList(countries, "Callingcode", "Data");

            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    var gym = new Gym();
                    user = new User();

                    if (this.User.Identity?.IsAuthenticated == true && this.User.IsInRole("Admin"))
                    {
                        var admin = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                        if (admin == null)
                        {
                            return UserNotFound();
                        }

                        gym = await _gymRepository.GetGymByUserAsync(admin);
                        if (gym == null)
                        {
                            return GymNotFound();
                        }
                    }

                    if (this.User.Identity?.IsAuthenticated == true && this.User.IsInRole("MasterAdmin"))
                    {
                        model.Role = "Admin";

                        gym = await _gymRepository.GetByIdTrackAsync(model.Gym);
                        if (gym == null)
                        {
                            return GymNotFound();
                        }
                    }

                    // Check if model.Role value is either one of these roles
                    if (!new[] { "Admin", "MasterAdmin", "Employee", "Instructor", "Client" }.Contains(model.Role))
                    {
                        return UserNotFound();
                    }

                    user = model.Role switch
                    {
                        "Admin" or "MasterAdmin" => new Admin { Gym = gym },
                        "Employee" => new Employee { Gym = gym },
                        "Instructor" => new Instructor { Gym = gym },
                        "Client" => new Client { Gym = gym },
                        _ => null
                    };

                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Email = model.Email;
                    user.UserName = model.Email;
                    user.BirthDate = model.BirthDate;
                    user.PhoneNumber = $"{model.CountryCallingcode}{model.PhoneNumber}";

                    if (_userHelper.CheckIfPhoneNumberExists(user.PhoneNumber))
                    {
                        ModelState.AddModelError("PhoneNumber", "Phone number already exists.");

                        return View(model);
                    }

                    string? password = "FitHub_2024";
                    var result = await _userHelper.AddUserAsync(user, password);

                    if (result.Succeeded)
                    {
                        if (model.Role == "Client")
                        {
                            var clientHistory = new ClientHistory()
                            {
                                Id = user.Id,
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                Email = user.Email,
                                BirthDate = user.BirthDate,
                                GymId = gym.Id,
                                PhoneNumber = user.PhoneNumber,
                            };

                            await _clientHistoryRepository.CreateAsync(clientHistory);
                        }
                        else
                        {
                            var staffHistory = new StaffHistory()
                            {
                                StaffId = user.Id,
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                Email = user.Email,
                                BirthDate = user.BirthDate,
                                GymId = gym.Id,
                                Role = model.Role,
                                PhoneNumber = user.PhoneNumber
                            };

                            await _staffHistoryRepository.CreateAsync(staffHistory);
                        }

                        await _userHelper.AddUserToRoleAsync(user, model.Role);

                        var userToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                        await _userHelper.ConfirmEmailAsync(user, userToken);

                        var resetToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

                        string? tokenLink = Url.Action("ConfirmEmailChangePassword", "Account", new
                        {
                            userid = user.Id,
                            token = resetToken
                        }, protocol: HttpContext.Request.Scheme);

                        string message = @$"
                        <table role=""presentation"" style=""width: 100%; border: 0; cellpadding: 0; cellspacing: 0;"">
                            <tr>
                                <td style=""padding: 10px 0; font-size: 15px"">
                                    Hey, {model.FirstName}, please click this button to configure your FitnessHub account:
                                </td>
                            </tr>
                            <tr>
                                <td style=""padding: 10px 0;"">
                                    <a href=""{tokenLink}"" style=""display: inline-block; background-color: black; color: white; font-size: 20px; font-weight: bold; padding: 10px 20px; text-decoration: none; border-radius: 5px; text-align: center;"">
                                        Configure
                                    </a>
                                </td>
                            </tr>
                        </table>";

                        string body = _mailHelper.GetEmailTemplate("Configure Account", message, $"Welcome to FitnessHub, {model.FirstName}");

                        Response response = await _mailHelper.SendEmailAsync(model.Email, "Account configuration", body, null, null);

                        if (response.IsSuccess)
                        {
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("Email", "This email is already registered");

                    return View(model);
                }
            }

            if (this.User.IsInRole("MasterAdmin"))
            {
                model.Gyms = _gymRepository.GetAll().Select(gym => new SelectListItem
                {
                    Value = gym.Id.ToString(),
                    Text = $"{gym.Data}",
                });

                _loadHelper.LoadMasterAdminRoles(model);
            }

            if (this.User.IsInRole("Admin"))
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                if (user == null)
                {
                    return UserNotFound();
                }

                var gym = await _gymRepository.GetGymByUserAsync(user);
                if (gym == null)
                {
                    return GymNotFound();
                }

                model.Gyms = new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Value = gym.Id.ToString(),
                        Text = $"{gym.Data}"
                    }
                };

                _loadHelper.LoadAdminRoles(model);
            }

            ModelState.AddModelError("", "Failed to create user.");

            return View(model);
        }

        // GET: Users/Edit/5
        [Authorize(Roles = "MasterAdmin, Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return UserNotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(id);
            if (user == null)
            {
                return UserNotFound();
            }

            var model = new AdminEditUserViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
            };

            if (this.User.IsInRole("MasterAdmin"))
            {
                if (user is Admin admin)
                {
                    model.GymId = admin.GymId.Value;
                    model.Gyms = _gymRepository.GetAll().Select(gym => new SelectListItem
                    {
                        Value = gym.Id.ToString(),
                        Text = $"{gym.Data}",

                    });
                }
            }

            if (this.User.IsInRole("Admin"))
            {
                if (user is Client client)
                {
                    var gym = await _gymRepository.GetByIdAsync(client.GymId);
                    if(gym == null)
                    {
                        return GymNotFound();
                    }

                    model.GymId = client.GymId;
                    model.Gyms = new List<SelectListItem>
                    {
                        new SelectListItem
                        {
                            Value = gym.Id.ToString(),
                            Text = gym.Data,
                            Selected = true,
                        }
                    };
                }

                if (user is Employee employee)
                {
                    model.GymId = employee.GymId.Value;
                    model.Gyms = _gymRepository.GetAll().Select(gym => new SelectListItem
                    {
                        Value = gym.Id.ToString(),
                        Text = $"{gym.Data}",

                    });
                }

                if (user is Instructor instructor)
                {
                    model.GymId = instructor.GymId.Value;
                    model.Gyms = _gymRepository.GetAll().Select(gym => new SelectListItem
                    {
                        Value = gym.Id.ToString(),
                        Text = $"{gym.Data}",
                    });
                }
            }

            return View(model);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminEditUserViewModel model)
        {
            if (model.GymId < 1)
            {
                ModelState.AddModelError("Gym", "Please select a gym.");
            }

            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByIdAsync(model.UserId);
                if (user == null)
                {
                    return UserNotFound();
                }

                var gym = await _gymRepository.GetByIdTrackAsync(model.GymId);
                if (gym == null)
                {
                    return GymNotFound();
                }

                user.Email = model.Email;
                user.UserName = model.Email;

                var role = string.Empty;

                if (user is Admin admin)
                {
                    admin.Gym = gym;
                    role = "Admin";
                }

                if (user is Instructor instructor)
                {
                    instructor.Gym = gym;
                    role = "Instructor";
                }

                if (user is Employee employee)
                {
                    employee.Gym = gym;
                    role = "Employee";
                }

                //if (user is Client client)
                //{
                //    client.Gym = gym;
                //}

                var result = await _userHelper.UpdateUserAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to update user.");
                    return View(user);
                }

                if (user is Client)
                {
                    var clientHistory = await _clientHistoryRepository.GetByIdTrackAsync(user.Id);
                    if (clientHistory == null)
                    {
                        return ClientHistoryNotFound();
                    }

                    //clientHistory.GymId = gym.Id;
                    clientHistory.Email = user.Email;

                    await _clientHistoryRepository.UpdateAsync(clientHistory);
                }

                var staffHistory = new StaffHistory
                {
                    StaffId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    BirthDate = user.BirthDate,
                    GymId = gym.Id,
                    Role = role,
                    PhoneNumber = user.PhoneNumber
                };

                staffHistory.GymId = gym.Id;
                staffHistory.Email = user.Email;

                await _staffHistoryRepository.UpdateAsync(staffHistory);

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        //GET: Users/Delete/5
        [Authorize(Roles = "MasterAdmin, Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return UserNotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(id);
            if (user == null)
            {
                return UserNotFound();
            }

            var userGym = await _gymRepository.GetGymByUserAsync(user);
            if (userGym == null)
            {
                return GymNotFound();
            }

            var roles = await _userHelper.GetUserRolesAsync(user);
            var role = roles.FirstOrDefault();

            var model = new UserDetailsViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Gym = userGym,
                Role = role,
                BirthDate = user.BirthDate,
                Avatar = user.Avatar,
                PhoneNumber = user.PhoneNumber,
            };

            return View(model);
        }

        //POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userHelper.GetUserByIdAsync(id);
            if (user == null)
            {
                return UserNotFound();
            }

            var userGym = await _gymRepository.GetGymByUserAsync(user);
            if (userGym == null)
            {
                return GymNotFound();
            }

            var roles = await _userHelper.GetUserRolesAsync(user);

            var role = roles.FirstOrDefault();
            
            var model = new UserDetailsViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Gym = userGym,
                Role = role,
                BirthDate = user.BirthDate,
                PhoneNumber = user.PhoneNumber,
                Avatar = user.Avatar,
            };

            if (user is Client client)
            {
                if (client.MembershipDetails != null)
                {
                    ModelState.AddModelError(string.Empty, "Cannot delete this client because he has an active membership.");
                    return View("Details", model);
                }

                if ((client.OnlineClass != null && client.OnlineClass.Any()) ||
                    (client.GymClass != null && client.GymClass.Any()))
                {
                    ModelState.AddModelError(string.Empty, "Cannot delete this client because he is enrolled in classes.");
                    return View("Details", model);
                }

                var result = await _userHelper.DeleteUser(client);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            if(user is Instructor instructor)
            {
                var gymClasses = await _classRepository.GetAllGymClassesInclude();
                if (gymClasses.Any())
                {
                    var instructorGymClasses = gymClasses.Where(c => c.Instructor.Id == instructor.Id).ToList();

                    if (instructorGymClasses.Any())
                    {
                        ModelState.AddModelError(string.Empty, "Cannot delete this instructor because he is assigned to classes.");
                        return View("Details", model);
                    }
                }

                var onlineClasses = await _classRepository.GetAllOnlineClassesInclude();
                if (onlineClasses.Any())
                {
                    var instructorOnlineClasses = onlineClasses.Where(c => c.Instructor.Id == instructor.Id).ToList();

                    if (instructorOnlineClasses.Any())
                    {
                        ModelState.AddModelError(string.Empty, "Cannot delete this instructor because he is assigned to classes.");
                        return View("Details", model);
                    }
                }

                var workouts = await _workoutRepository.GetAllWorkoutsIncludeAsync();
                if(workouts.Any())
                {
                    var instructorWorkouts = workouts.Where(w => w.Instructor.Id == instructor.Id);
                    if (instructorWorkouts.Any())
                    {
                        ModelState.AddModelError(string.Empty, "Cannot delete this instructor because he has assigned workouts.");
                        return View("Details", model);
                    }
                }

                var result = await _userHelper.DeleteUser(instructor);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            if(user is Employee employee)
            {
                var result = await _userHelper.DeleteUser(employee);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            if (user is Admin admin)
            {
                var result = await _userHelper.DeleteUser(admin);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            ModelState.AddModelError(string.Empty, "Couldn't delete the user.");
            return View("Details", model);
        }

        [Authorize(Roles = "MasterAdmin, Admin")]
        public async Task<IActionResult> ClientsHistory()
        {
            var clientsHistory = new List<ClientHistory>();
            var clientsHistoryModel = new List<ClientHistoryViewModel>();

            if (this.User.IsInRole("MasterAdmin"))
            {
                clientsHistory = _clientHistoryRepository.GetAll().ToList();
            }

            if (this.User.IsInRole("Admin"))
            {
                var admin = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

                if (admin == null)
                {
                    return UserNotFound();
                }

                var adminGym = await _gymRepository.GetGymByUserAsync(admin);

                if (adminGym == null)
                {
                    return GymNotFound();
                }

                clientsHistory = _clientHistoryRepository.GetByGymId(adminGym.Id).ToList();
            }

            foreach (var client in clientsHistory)
            {
                var gym = await _gymHistoryRepository.GetByIdAsync(client.GymId);
                if (gym == null)
                {
                    return GymNotFound();
                }

                clientsHistoryModel.Add(new ClientHistoryViewModel()
                {
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    BirthDate = client.BirthDate,
                    Email = client.Email,
                    Gym = gym.Name,
                    PhoneNumber = client.PhoneNumber,
                });
            }
            return View(clientsHistoryModel);
        }

        [Authorize(Roles = "MasterAdmin, Admin")]
        public async Task<IActionResult> StaffHistory()
        {
            var staffHistory = new List<StaffHistory>();
            var staffHistoryModel = new List<StaffHistoryViewModel>();

            if (this.User.IsInRole("MasterAdmin"))
            {
                staffHistory = _staffHistoryRepository.GetAll().ToList();
            }

            if (this.User.IsInRole("Admin"))
            {
                var admin = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                if (admin == null)
                {
                    return UserNotFound();
                }

                var adminGym = await _gymRepository.GetGymByUserAsync(admin);
                if (adminGym == null)
                {
                    return GymNotFound();
                }

                staffHistory = _staffHistoryRepository.GetByGymId(adminGym.Id).ToList();
            }

            foreach (var staff in staffHistory)
            {
                var gym = await _gymHistoryRepository.GetByIdAsync(staff.GymId);
                if (gym == null)
                {
                    return GymNotFound();
                }

                staffHistoryModel.Add(new StaffHistoryViewModel()
                {
                    FirstName = staff.FirstName,
                    LastName = staff.LastName,
                    BirthDate = staff.BirthDate,
                    Email = staff.Email,
                    Gym = gym.Name,
                    Role = staff.Role,
                    PhoneNumber = staff.PhoneNumber,
                });
            }

            return View(staffHistoryModel);
        }

        public IActionResult UserNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "User not found", Message = "Looks like this user skipped leg day!" });
        }

        public IActionResult GymNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Gym not found", Message = "With so many worldwide, how did you miss this one?" });
        }

        public IActionResult ClientHistoryNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Client history not found", Message = "No history found for that client." });
        }

        public IActionResult StaffHistoryNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Staff history not found", Message = "No history found for that staff." });
        }
    }
}