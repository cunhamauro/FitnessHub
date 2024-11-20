﻿using FitnessHub.Data.Classes;
using FitnessHub.Data.Entities.History;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FitnessHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IImageHelper _imageHelper;
        private readonly ILoadHelper _loadHelper;
        private readonly IConfiguration _configuration;
        private readonly IGymRepository _gymRepository;
        private readonly IClientHistoryRepository _clientHistoryRepository;

        public AccountController(
            IUserHelper userHelper,
            IMailHelper mailHelper,
            IImageHelper imageHelper,
            ILoadHelper loadHelper,
            IConfiguration configuration,
            IGymRepository gymRepository,
            IClientHistoryRepository clientHistoryRepository)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _imageHelper = imageHelper;
            _loadHelper = loadHelper;
            _configuration = configuration;
            _gymRepository = gymRepository;
            _clientHistoryRepository = clientHistoryRepository;
        }

        public IActionResult Login()
        {
            return User.Identity.IsAuthenticated ? RedirectToAction("Index", "Home") : View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userHelper.LoginAsync(model);
                if (result.Succeeded)
                {
                    if (this.Request.Query.Keys.Contains("ReturnUrl"))
                    {
                        return Redirect(this.Request.Query["ReturnUrl"].First());
                    }

                    return this.RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("Username", "Failed to login");

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Register()
        {
            var countries = await _loadHelper.LoadCountriesAsync();

            var model = new RegisterNewUserViewModel()
            {
                Gyms = _gymRepository.GetAll().Select(gym => new SelectListItem
                {
                    Value = gym.Id.ToString(),
                    Text = $"{gym.Data}",
                }),
                Countries = new SelectList(countries, "Callingcode", "Data")
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterNewUserViewModel model)
        {
            if (model.Gym < 1)
            {
                ModelState.AddModelError("Gym", "Please select a gym.");
            }

            if (model.CountryCallingcode == "0" || model.CountryCallingcode == "undefined")
            {
                ModelState.AddModelError("PhoneNumber", "Please select a country.");
            }

            var countries = await _loadHelper.LoadCountriesAsync();

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

                    user = new Client
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        UserName = model.Email,
                        BirthDate = model.BirthDate,
                        Gym = gym,
                        PhoneNumber = $"{model.CountryCallingcode}{model.PhoneNumber}"
                    };

                    if (_userHelper.CheckIfPhoneNumberExists(user.PhoneNumber))
                    {
                        ModelState.AddModelError("PhoneNumber", "Phone number already exists.");

                        model.Gyms = _gymRepository.GetAll().Select(gym => new SelectListItem
                        {
                            Value = gym.Id.ToString(),
                            Text = $"{gym.Data}",
                        });

                        model.Countries = new SelectList(countries, "Callingcode", "Data");

                        return View(model);
                    }

                    var result = await _userHelper.AddUserAsync(user, model.Password);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }

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

                    await _userHelper.AddUserToRoleAsync(user, "Client");

                    string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);

                    string? tokenLink = Url.Action("ConfirmEmail", "Account", new
                    {
                        userid = user.Id,
                        token = myToken
                    }, protocol: HttpContext.Request.Scheme);

                    string message = @$"
                        <table role=""presentation"" style=""width: 100%; border: 0; cellpadding: 0; cellspacing: 0;"">
                            <tr>
                                <td style=""padding: 10px 0; font-size: 15px"">
                                    Please click this button to confirm your FitnessHub account:
                                </td>
                            </tr>
                            <tr>
                                <td style=""padding: 10px 0;"">
                                    <a href=""{tokenLink}"" style=""display: inline-block; background-color: black; color: white; font-size: 20px; font-weight: bold; padding: 10px 20px; text-decoration: none; border-radius: 5px; text-align: center;"">
                                        Confirm
                                    </a>
                                </td>
                            </tr>
                        </table>";

                    string body = _mailHelper.GetEmailTemplate("Confirm Account", message, $"Welcome to FitnessHub, {model.FirstName}");

                    Response response = await _mailHelper.SendEmailAsync(model.Email, "Account confirmation", body, null, null);

                    //Response response = await _mailHelper.SendEmailAsync(model.Email, "Email confirmation", $"<h1>Email Confirmation</h1>" +
                    //    $"To allow the user, " +
                    //    $"plase click in this link:</br></br><a href = \"{tokenLink}\">Confirm Email</a>");

                    if (response.IsSuccess)
                    {
                        ViewBag.Message = "The instructions to confirm your account have been sent to your email";
                        return View(model);
                    }

                    return DisplayMessage("Email not sent", "There was an error sending the email to confirm the account. Try again later!");
                }
                else
                {
                    ModelState.AddModelError("Email", "This email is already registered");

                    return View(model);
                }
            }

            model.Gyms = _gymRepository.GetAll().Select(gym => new SelectListItem
            {
                Value = gym.Id.ToString(),
                Text = $"{gym.Data}",
            });

            model.Countries = new SelectList(countries, "Callingcode", "Data");

            return View(model);
        }

        public async Task<IActionResult> ChangeUser()
        {
            var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

            if (user == null)
            {
                return UserNotFound();
            }

            var countries = await _loadHelper.LoadCountriesAsync();

            var model = new ChangeUserViewModel()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                BirthDate = user.BirthDate,
                PhoneNumber = user.PhoneNumber,
                Countries = new SelectList(countries, "Callingcode", "Data"),
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUser(ChangeUserViewModel model)
        {
            var countries = await _loadHelper.LoadCountriesAsync();

            if (model.CountryCallingcode == "0" || model.CountryCallingcode == "undefined")
            {
                ModelState.AddModelError("PhoneNumber", "Please select a country.");
            }

            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                if (user != null)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.BirthDate = model.BirthDate;
                    user.PhoneNumber = $"{model.CountryCallingcode}{model.PhoneNumber}";

                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        var path = await _imageHelper.UploadImageAsync(model.ImageFile, "users");
                        user.ImagePath = path;
                    }

                    if (_userHelper.CheckIfPhoneNumberExists(user.PhoneNumber))
                    {
                        ModelState.AddModelError("PhoneNumber", "Phone number already exists.");

                        model.Countries = new SelectList(countries, "Callingcode", "Data");

                        return View(model);
                    }

                    var response = await _userHelper.UpdateUserAsync(user);

                    if (response.Succeeded)
                    {
                        var clientHistory = await _clientHistoryRepository.GetByIdTrackAsync(user.Id);
                        if (clientHistory == null)
                        {
                            return ClientHistoryNotFound();
                        }

                        clientHistory.FirstName = user.FirstName;
                        clientHistory.LastName = user.LastName;
                        clientHistory.BirthDate = user.BirthDate;
                        clientHistory.PhoneNumber = user.PhoneNumber;

                        await _clientHistoryRepository.UpdateAsync(clientHistory);

                        ViewBag.UserMessage = "Successfully updated!";
                    }
                    else
                    {
                        return DisplayMessage("Error updating account", "The account could not be updated. Try again!");
                    }
                }
                else
                {
                    return UserNotFound();
                }
            }

            model.Countries = new SelectList(countries, "Callingcode", "Data");

            return View(model);
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

                if (user != null)
                {
                    var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }

                    if (result.Succeeded)
                    {
                        return RedirectToAction("ChangeUser");
                    }
                    else
                    {
                        return DisplayMessage("Error changing password", "The password could not be updated. Try again!");
                    }
                }
                else
                {
                    return UserNotFound();
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateToken([FromBody] LoginViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Username);
                if (user != null)
                {
                    var result = await _userHelper.ValidatePasswordAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
                        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var token = new JwtSecurityToken(
                            _configuration["Tokens:Issuer"],
                            _configuration["Tokens:Audience"],
                            claims,
                            expires: DateTime.UtcNow.AddDays(15),
                            signingCredentials: credentials);
                        var results = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        };

                        return this.Created(string.Empty, results);
                    }
                }
            }

            return BadRequest();
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return UserNotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null)
            {
                return UserNotFound();
            }

            var result = await _userHelper.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                return DisplayMessage("Email confirmation failure", "Your account activation has failed! Try again!");
            }

            return RedirectToAction("Login");
        }

        public IActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "The email doesn't correspond to a registered user");
                    return View(model);
                }

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

                var tokenLink = this.Url.Action(
                    "ResetPassword",
                    "Account",
                    new { token = myToken }, protocol: HttpContext.Request.Scheme);

                string message = @$"
                        <table role=""presentation"" style=""width: 100%; border: 0; cellpadding: 0; cellspacing: 0;"">
                            <tr>
                                <td style=""padding: 10px 0; font-size: 15px"">
                                    Hey, {user.FirstName}, please click this button to recover your FitnessHub account:
                                </td>
                            </tr>
                            <tr>
                                <td style=""padding: 10px 0;"">
                                    <a href=""{tokenLink}"" style=""display: inline-block; background-color: black; color: white; font-size: 20px; font-weight: bold; padding: 10px 20px; text-decoration: none; border-radius: 5px; text-align: center;"">
                                        Recover
                                    </a>
                                </td>
                            </tr>
                        </table>";

                string body = _mailHelper.GetEmailTemplate("Recover Account", message, $"Don't forget your password again");

                Response response = await _mailHelper.SendEmailAsync(model.Email, "Account recovery", body, null, null);


                if (response.IsSuccess)
                {
                    ViewBag.Message = "The instructions to recover your password have been sent to your email";
                }

                return View();
            }

            return View(model);
        }

        public IActionResult ResetPassword(string token)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _userHelper.GetUserByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Login));
                }

                ModelState.AddModelError("", "Error while resetting the password");

                return View(model);
            }

            ModelState.AddModelError("", "User not found");

            return View(model);
        }

        public async Task<IActionResult> ConfirmEmailChangePassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return UserNotFound();
            }

            if (User.Identity.IsAuthenticated)
            {
                await _userHelper.LogoutAsync();
                return RedirectToAction(nameof(ConfirmEmailChangePassword), new { userId, token });
            }

            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null)
            {
                return UserNotFound();
            }

            var model = new ResetPasswordViewModel
            {
                Email = user.Email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEmailChangePassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userHelper.GetUserByEmailAsync(model.Email);

            if (user != null)
            {
                var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                if (result.Succeeded)
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    return DisplayMessage("Password configuration failed", "Your password configuration has failed! Try again.");
                }
            }
            else
            {
                return UserNotFound();
            }
        }

        // GET: Account/Clients
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Clients()
        {
            var employee = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
            if (employee == null)
            {
                return UserNotFound();
            }

            var gym = await _gymRepository.GetGymByUserAsync(employee);
            if (gym == null)
            {
                return GymNotFound();
            }

            var clients = await _userHelper.GetClientsByGymAsync(gym.Id);

            return View(clients.ToList());
        }

        // GET: Account/RegisterNewClient
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> RegisterNewClient()
        {
            var model = new EmployeeRegisterNewClientViewModel();

            var countries = await _loadHelper.LoadCountriesAsync();
            model.Countries = new SelectList(countries, "Callingcode", "Data");

            return View(model);
        }

        // POST: Account/RegisterNewClient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterNewClient(EmployeeRegisterNewClientViewModel model)
        {
            if (model.CountryCallingcode == "0" || model.CountryCallingcode == "undefined")
            {
                ModelState.AddModelError("PhoneNumber", "Please select a country.");
            }

            var countries = await _loadHelper.LoadCountriesAsync();

            if (ModelState.IsValid)
            {
                var employee = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                if (employee == null)
                {
                    return UserNotFound();
                }

                var user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    var gym = await _gymRepository.GetGymByUserAsync(employee);
                    if (gym == null)
                    {
                        return GymNotFound();
                    }

                    user = new Client
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        UserName = model.Email,
                        BirthDate = model.BirthDate,
                        Gym = gym,
                        PhoneNumber = $"{model.CountryCallingcode}{model.PhoneNumber}"
                    };

                    if (_userHelper.CheckIfPhoneNumberExists(user.PhoneNumber))
                    {
                        ModelState.AddModelError("PhoneNumber", "Phone number already exists.");

                        model.Countries = new SelectList(countries, "Callingcode", "Data");

                        return View(model);
                    }

                    string? password = "FitHub_2024";
                    var result = await _userHelper.AddUserAsync(user, password);

                    if (result.Succeeded)
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

                        await _userHelper.AddUserToRoleAsync(user, "Client");
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
                            return RedirectToAction(nameof(Index), "Home");
                        }
                    }
                }

                ModelState.AddModelError("Email", "Email already registered.");
            }

            ModelState.AddModelError("", "Failed to create user.");

            model.Countries = new SelectList(countries, "Callingcode", "Data");

            return View(model);
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

        public IActionResult NotAuthorized()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Not authorized", Message = $"You haven't warmed up enough for this!" });
        }

        public IActionResult DisplayMessage(string title, string message)
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = $"{title}", Message = $"{message}" });
        }
    }
}