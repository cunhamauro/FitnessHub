using FitnessHub.Data.Classes;
using FitnessHub.Data.Entities.History;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models.API;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace FitnessHub.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IConfiguration _configuration;
        private readonly IClientHistoryRepository _clientHistoryRepository;
        private string _baseUrl = string.Empty;

        public ClientsController(
            IUserHelper userHelper,
            IMailHelper mailHelper,
            IImageHelper imageHelper,
            IConfiguration configuration,
            IClientHistoryRepository clientHistoryRepository)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _imageHelper = imageHelper;
            _configuration = configuration;
            _clientHistoryRepository = clientHistoryRepository;

            _baseUrl = _configuration["AppSettings:Url"];
        }

        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = await _userHelper.GetUserByEmailAsync(model.Email) as Client;
            if (user != null)
            {
                return BadRequest("There is already a user registered with this email.");
            }

            user = new Client
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                BirthDate = model.BirthDate,
                PhoneNumber = model.PhoneNumber,
                GymId = model.GymId,
            };

            if (_userHelper.CheckIfPhoneNumberExists(user.PhoneNumber))
            {
                return BadRequest("Phone number already exists");
            }

            var result = await _userHelper.AddUserAsync(user, model.Password);
            if (result != IdentityResult.Success)
            {
                return BadRequest("Couldn't register user");
            }

            var clientHistory = new ClientHistory()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                BirthDate = user.BirthDate,
                GymId = user.GymId,
                PhoneNumber = user.PhoneNumber,
            };

            await _clientHistoryRepository.CreateAsync(clientHistory);

            await _userHelper.AddUserToRoleAsync(user, "Client");

            string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
            if (myToken == null)
            {
                return BadRequest("Couldn't generate token");
            }

            string tokenLink = $"{_baseUrl}api/clients/confirmemail?userid={user.Id}&token={WebUtility.UrlEncode(myToken)}";

            Response response = await _mailHelper.SendEmailAsync(model.Email, "Email confirmation", $"<h1>Email Confirmation</h1>" +
                        $"To finalize the register, " +
                        $"plase click in this link:</br></br><a href = \"{tokenLink}\">Confirm Email</a>", null, null);

            if (response.IsSuccess)
            {
                return Ok("Check your email to finalize the register");
            }

            return BadRequest("Failed to send email with token");
        }

        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userHelper.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return BadRequest("Invalid email confirmation token.");
            }

            if (this.User.Identity.IsAuthenticated)
            {
                await _userHelper.LogoutAsync();
            }

            return Redirect($"{_baseUrl}Account/Login");
        }

        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userHelper.GetUserByEmailAsync(model.Email);
            if (user == null || !await _userHelper.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized("Invalid email or password.");
            }

            if (user is not Client)
            {
                return Unauthorized("User is not client.");
            }

            var key = _configuration["Tokens:Key"] ?? throw new ArgumentNullException("Tokens:Key", "Tokens:Key cannot be null.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, model.Email!)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Tokens:Issuer"],
                audience: _configuration["Tokens:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(10),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return new ObjectResult(new
            {
                AccessToken = jwt,
                TokenType = "bearer",
                UserId = user.Id,
                UserName = user.UserName
            });
        }

        [HttpPost("[action]")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadUserImage(IFormFile image)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var user = await _userHelper.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound(new { ErrorMessage = "User not found" });
            }

            if (image != null)
            {
                user.ImagePath = await _imageHelper.UploadImageAsync(image, "users");

                var response = await _userHelper.UpdateUserAsync(user);
                if (response.Succeeded)
                {
                    return Ok(new { Message = "Image uploaded successfully." });
                }
            }

            return BadRequest(new { ErrorMessage = "No image uploaded." });
        }

        [HttpGet("UserImage")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserImage()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var user = await _userHelper.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound(new { ErrorMessage = "User not found" });
            }

            var userImage = await _userHelper.GetUserImageAsync(userEmail);

            return Ok(userImage);
        }

        [HttpGet("UserInfo")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserInfo()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var user = await _userHelper.GetUserByEmailAsync(userEmail) as Client;
            if (user == null)
            {
                return NotFound(new { ErrorMessage = "User not found" });
            }

            var result = new
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                BirthDate = user.BirthDate,
                PhoneNumber = user.PhoneNumber,
                GymId = user.GymId
            };

            return Ok(result);
        }

        [HttpPut("[action]")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUserInfo(UpdateUserModel model)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var user = await _userHelper.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound(new { ErrorMessage = "User not found" });
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.BirthDate = model.BirthDate;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userHelper.UpdateUserAsync(user);
            if (result.Succeeded)
            {
                var clientHistory = await _clientHistoryRepository.GetByIdTrackAsync(user.Id);
                if (clientHistory == null)
                {
                    return NotFound(new { ErrorMessage = "Client history not found for this user" });
                }

                clientHistory.FirstName = user.FirstName;
                clientHistory.LastName = user.LastName;
                clientHistory.BirthDate = user.BirthDate;
                clientHistory.PhoneNumber = user.PhoneNumber;

                await _clientHistoryRepository.UpdateAsync(clientHistory);

                return Ok(new { Message = "User information updated successfully." });
            }

            return BadRequest(new { ErrorMessage = "Couldn't update user information." });
        }

        [HttpPost("[action]")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var user = await _userHelper.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound(new { ErrorMessage = "User not found" });
            }

            var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Password changed successfully!" });
            }

            return BadRequest(new { ErrorMessage = "Couldn't change password." });
        }

        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordModel model)
        {
            var user = await _userHelper.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound(new { ErrorMessage = "User not found" });
            }

            var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
            if (myToken == null)
            {
                return BadRequest(new { ErrorMessage = "Couldn't generate token." });
            }

            Response response = await _mailHelper.SendEmailAsync(model.Email, "FitnessHub Password Reset", $"<h1>FitnessHub Password Reset</h1>" +
                    $"To reset your password use this token:</br></br> {myToken}", null, null);
            if (response.IsSuccess)
            {
                return Ok(new { Message = "The token to recover your password has been sent to your email." });
            }

            return BadRequest(new { ErrorMessage = "Failed to send email with token." });
        }

        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            var user = await _userHelper.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound(new { ErrorMessage = "User not found" });
            }

            var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Password reset successfully!" });
            }

            return BadRequest(new { ErrorMessage = "Couldn't reset password." });
        }
    }
}
