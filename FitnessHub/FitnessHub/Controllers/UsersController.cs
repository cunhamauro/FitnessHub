using FitnessHub.Data.Classes;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace FitnessHub.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly ILoadRolesHelper _loadRolesHelper;

        public UsersController(IUserHelper userHelper, IMailHelper mailHelper, ILoadRolesHelper loadRolesHelper)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _loadRolesHelper = loadRolesHelper;
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
                users = (await _userHelper.GetEmployeesAndInstructorsAndClientsAsync()).ToList();
            }

            var usersList = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var role = (await _userHelper.GetUserRolesAsync(user)).FirstOrDefault();

                usersList.Add(new UserRoleViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = role
                });
            }

            return View(usersList);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            var model = new AdminRegisterNewUserViewModel();
            
            if (this.User.IsInRole("MasterAdmin"))
            {
                _loadRolesHelper.LoadMasterAdminRoles(model);
            }

            if (this.User.IsInRole("Admin"))
            {
                _loadRolesHelper.LoadAdminRoles(model);
            }

            return View(model);
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminRegisterNewUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Username);
                if (user == null)
                {
                    if (model.SelectedRole == "Admin" || model.SelectedRole == "MasterAdmin")
                    {
                        user = new Admin();
                    }
                    if (model.SelectedRole == "Employee")
                    {
                        user = new Employee();
                    }
                    if (model.SelectedRole == "Client")
                    {
                        user = new Client();
                    }
                    if (model.SelectedRole == "Instructor")
                    {
                        user = new Instructor();
                    }

                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Email = model.Username;
                    user.UserName = model.Username;
                    user.BirthDate = model.BirthDate;

                    string? password = "FitHub_2024";
                    var result = await _userHelper.AddUserAsync(user, password);

                    if (result.Succeeded)
                    {
                        await _userHelper.AddUserToRoleAsync(user, model.SelectedRole);
                        var userToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                        await _userHelper.ConfirmEmailAsync(user, userToken);

                        var resetToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

                        string? tokenLink = Url.Action("ConfirmEmailChangePassword", "Account", new
                        {
                            userid = user.Id,
                            token = resetToken
                        }, protocol: HttpContext.Request.Scheme);

                        Response response = await _mailHelper.SendEmailAsync(model.Username, "Email confirmation", $"<h1>Email Confirmation</h1>" +
                        $"To allow the user, " +
                        $"plase click in this link:</br></br><a href = \"{tokenLink}\">Click here to confirm your  email and change your password</a>");

                        if (response.IsSuccess)
                        {
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }

            if (this.User.IsInRole("MasterAdmin"))
            {
                _loadRolesHelper.LoadMasterAdminRoles(model);
            }

            if (this.User.IsInRole("Admin"))
            {
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

            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByIdAsync(model.Id);
                if (user == null)
                {
                    return UserNotFound();
                }

                user.Email = model.Email;
                user.UserName = model.Email;

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

            return View(user);
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
    }
}
