using FitnessHub.Data.Classes;
using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.History;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessHub.Controllers
{
    [Authorize(Roles = "MasterAdmin, Admin")]
    public class UsersController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly ILoadRolesHelper _loadRolesHelper;
        private readonly IGymRepository _gymRepository;
        private readonly IClientHistoryRepository _clientHistoryRepository;
        private readonly IStaffHistoryRepository _staffHistoryRepository;

        public UsersController(
            IUserHelper userHelper,
            IMailHelper mailHelper,
            ILoadRolesHelper loadRolesHelper,
            IGymRepository gymRepository,
            IClientHistoryRepository clientHistoryRepository,
            IStaffHistoryRepository staffHistoryRepository)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _loadRolesHelper = loadRolesHelper;
            _gymRepository = gymRepository;
            _clientHistoryRepository = clientHistoryRepository;
            _staffHistoryRepository = staffHistoryRepository;
        }

        // GET: Users
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
                    Gym = gym
                });
            }

            return View(usersList);
        }

        // GET: Users/Details/5
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
                Avatar = user.Avatar,
            };

            return View(model);
        }

        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            var model = new AdminRegisterNewUserViewModel();

            if (this.User.IsInRole("MasterAdmin"))
            {
                model.Gyms = _gymRepository.GetAll().Select(gym => new SelectListItem
                {
                    Value = gym.Id.ToString(),
                    Text = $"{gym.Data}",
                });

                _loadRolesHelper.LoadMasterAdminRoles(model);
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

                _loadRolesHelper.LoadAdminRoles(model);
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

            if (this.User.IsInRole("Admin"))
            {
                ModelState.Remove("Gym");
            }

            if (this.User.IsInRole("MasterAdmin"))
            {
                ModelState.Remove("Role");
            }

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

                    string? password = "FitHub_2024";
                    var result = await _userHelper.AddUserAsync(user, password);

                    if (result.Succeeded)
                    {
                        if(model.Role == "Client")
                        {
                            var clientHistory = new ClientHistory()
                            {
                                Id = user.Id,
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                Email = user.Email,
                                BirthDate = user.BirthDate,
                                GymId = gym.Id,
                            };

                            await _clientHistoryRepository.CreateAsync(clientHistory);
                        }
                        else
                        {
                            var staffHistory = new StaffHistory()
                            {
                                Id = user.Id,
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                Email = user.Email,
                                BirthDate = user.BirthDate,
                                GymId = gym.Id,
                                Role = model.Role,
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

                        Response response = await _mailHelper.SendEmailAsync(model.Email, "Email confirmation", $"<h1>Email Confirmation</h1>" +
                            $"To allow the user, " +
                            $"plase click in this link:</br></br><a href = \"{tokenLink}\">Click here to confirm your  email and change your password</a>");

                        if (response.IsSuccess)
                        {
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("Email", "This email is already registered");

                    model.Gyms = _gymRepository.GetAll().Select(gym => new SelectListItem
                    {
                        Value = gym.Id.ToString(),
                        Text = $"{gym.Data}",
                    });

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

                _loadRolesHelper.LoadMasterAdminRoles(model);
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

                _loadRolesHelper.LoadAdminRoles(model);
            }

            ModelState.AddModelError("", "Failed to create user.");

            model.Gyms = _gymRepository.GetAll().Select(gym => new SelectListItem
            {
                Value = gym.Id.ToString(),
                Text = $"{gym.Data}",
            });

            return View(model);
        }

        // GET: Users/Edit/5
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
                Id = user.Id,
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
                    model.GymId = client.GymId.Value;
                    model.Gyms = _gymRepository.GetAll().Select(gym => new SelectListItem
                    {
                        Value = gym.Id.ToString(),
                        Text = $"{gym.Data}",

                    });
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
                var user = await _userHelper.GetUserByIdAsync(model.Id);
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

                if (user is Admin admin)
                {
                    admin.Gym = gym;
                }

                if (user is Instructor instructor)
                {
                    instructor.Gym = gym;
                }

                if (user is Employee employee)
                {
                    employee.Gym = gym;
                }

                if (user is Client client)
                {
                    client.Gym = gym;
                }

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

                    clientHistory.GymId = gym.Id;
                    clientHistory.Email = user.Email;

                    await _clientHistoryRepository.UpdateAsync(clientHistory);
                }

                var staffHistory = await _staffHistoryRepository.GetByIdTrackAsync(user.Id);
                if (staffHistory == null)
                {
                    return StaffHistoryNotFound();
                }

                staffHistory.GymId = gym.Id;
                staffHistory.Email = user.Email;

                await _staffHistoryRepository.UpdateAsync(staffHistory);

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        //GET: Users/Delete/5
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
            };

            return View(model);
        }

        //POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userHelper.GetUserByIdAsync(id);

            if (user != null)
            {
                var result = await _userHelper.DeleteUser(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(user);
        }

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
                var user = await _userHelper.GetUserByIdAsync(client.Id);
                if (user == null)
                {
                    return UserNotFound();
                }

                var gym = await _gymRepository.GetGymByUserAsync(user);
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
                    Gym = gym.Name
                });
            }

            return View(clientsHistoryModel);
        }

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
                var user = await _userHelper.GetUserByIdAsync(staff.Id);
                if (user == null)
                {
                    return UserNotFound();
                }

                var gym = await _gymRepository.GetGymByUserAsync(user);
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
