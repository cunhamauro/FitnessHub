using FitnessHub.Data.Classes;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Helpers;
using FitnessHub.Models.API;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FitnessHub.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly string _baseUrl = "https://localhost:44370/";

        public ClientsController(IUserHelper userHelper, IMailHelper mailHelper)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
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

            var result = await _userHelper.AddUserAsync(user, model.Password);
            if (result != IdentityResult.Success)
            {
                return BadRequest("Couldn't register user.");
            }

            await _userHelper.AddUserToRoleAsync(user, "Client");

            string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
            if (myToken == null)
            {
                return BadRequest("Couldn't generate token.");
            }

            string tokenLink = $"{_baseUrl}api/clients/confirmemail?userid={user.Id}&token={WebUtility.UrlEncode(myToken)}";

            Response response = await _mailHelper.SendEmailAsync(model.Email, "Email confirmation", $"<h1>Email Confirmation</h1>" +
                        $"To finalize the register, " +
                        $"plase click in this link:</br></br><a href = \"{tokenLink}\">Confirm Email</a>");

            if (response.IsSuccess)
            {
                return Ok("Check your email to finalize the register.");
            }

            return BadRequest("Failed to send email with token.");
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userHelper.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return Redirect($"{_baseUrl}Account/Login");
            }

            return BadRequest("Invalid email confirmation token.");
        }
    }
}
