using FitnessHub.Data.Classes;
using FitnessHub.Data.Entities;
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

        public UsersController(
            IUserHelper userHelper, 
            IMailHelper mailHelper, 
            ILoadRolesHelper loadRolesHelper, 
            IGymRepository gymRepository)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _loadRolesHelper = loadRolesHelper;
            _gymRepository = gymRepository;
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
            if(userGym == null)
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
                if(user == null)
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

            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    var gym = await _gymRepository.GetByIdTrackAsync(model.Gym);
                    if (gym == null)
                    {
                        return GymNotFound();
                    }

                    if (model.Role == "Admin" || model.Role == "MasterAdmin")
                    {
                        user = new Admin
                        { 
                            Gym = gym
                        };
                    }

                    if (model.Role == "Employee")
                    {
                        user = new Employee
                        {
                            Gym = gym
                        };
                    }
                    
                    if (model.Role == "Instructor")
                    {
                        user = new Instructor
                        {
                            Gym = gym
                        };
                    }

                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Email = model.Email;
                    user.UserName = model.Email;
                    user.BirthDate = model.BirthDate;

                    string? password = "FitHub_2024";
                    var result = await _userHelper.AddUserAsync(user, password);

                    if (result.Succeeded)
                    {
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
                if(user is Admin admin)
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
                if(gym == null)
                {
                    return GymNotFound();
                }

                user.Email = model.Email;
                user.UserName = model.Email;
                
                if(user is Admin admin)
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
