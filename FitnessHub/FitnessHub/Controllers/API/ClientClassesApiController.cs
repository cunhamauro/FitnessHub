using FitnessHub.Data.Classes;
using FitnessHub.Data.Entities.GymClasses;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models.API;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FitnessHub.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientClassesApiController : ControllerBase
    {
        private readonly IUserHelper _userHelper;
        private readonly IMembershipDetailsRepository _membershipDetailsRepository;
        private readonly IClassRepository _classRepository;
        private readonly IRegisteredInClassesHistoryRepository _registeredInClassesHistoryRepository;
        private readonly IClassHistoryRepository _classHistoryRepository;
        private readonly IGymHistoryRepository _gymHistoryRepository;
        private readonly IStaffHistoryRepository _staffHistoryRepository;
        private readonly IClassTypeRepository _classTypeRepository;
        private readonly IMailHelper _mailHelper;

        public ClientClassesApiController(
            IUserHelper userHelper,
            IMembershipDetailsRepository membershipDetailsRepository,
            IClassRepository classRepository,
            IRegisteredInClassesHistoryRepository registeredInClassesHistoryRepository,
            IClassHistoryRepository classHistoryRepository,
            IGymHistoryRepository gymHistoryRepository,
            IStaffHistoryRepository staffHistoryRepository,
            IClassTypeRepository classTypeRepository, IMailHelper mailHelper)
        {
            _userHelper = userHelper;
            _membershipDetailsRepository = membershipDetailsRepository;
            _classRepository = classRepository;
            _registeredInClassesHistoryRepository = registeredInClassesHistoryRepository;
            _classHistoryRepository = classHistoryRepository;
            _gymHistoryRepository = gymHistoryRepository;
            _staffHistoryRepository = staffHistoryRepository;
            _classTypeRepository = classTypeRepository;
            _mailHelper = mailHelper;
        }

        [HttpPost("[action]")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterInClass([FromBody] RegisterInClassModel register)
        {
            var classId = register.ClassId;
            var isOnline = register.IsOnline;

            var clientEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var client = await _userHelper.GetUserByEmailAsync(clientEmail) as Client;
            if (client == null)
            {
                return NotFound(new { ErrorMessage = "User not found" });
            }

            if (client.MembershipDetailsId == null)
            {
                return BadRequest(new { ErrorMessage = "Client has no membership. Sign up, for a membership, on our website..." });
            }

            var memberShipDetailClient = await _membershipDetailsRepository.GetByIdAsync(client.MembershipDetailsId.Value);

            if (memberShipDetailClient == null)
            {
                return BadRequest(new { ErrorMessage = "Membership details not found" });
            }

            if (memberShipDetailClient.Status == false)
            {
                return BadRequest(new { ErrorMessage = "Membership is not active" });
            }

            var history = new RegisteredInClassesHistory
            {
                UserId = client.Id,
                ClassId = classId,
                RegistrationDate = DateTime.UtcNow,
                Canceled = false,
            };

            string classesUrl = Url.Action("AvailableClasses", "Classes");

            if (isOnline)
            {
                var onlineClass = await _classRepository.GetOnlineClassByIdInclude(classId);

                if (onlineClass == null)
                {
                    return NotFound("Online class not found");
                }

                if (memberShipDetailClient.DateRenewal <= onlineClass.DateStart)
                {
                    return BadRequest(new { ErrorMessage = "Class starts after memberhip's date of renewal" });
                }

                if (!onlineClass.Clients.Any(c => c.Id == client.Id))
                {
                    onlineClass.Clients.Add(client);
                    await _classRepository.UpdateAsync(onlineClass);
                    await _registeredInClassesHistoryRepository.CreateAsync(history);

                    var body = _mailHelper.GetEmailTemplate($"Registered in {onlineClass.ClassType.Name} Online Class", @$"Hey, {client.FirstName}, you registered yourself in the <span style=""font-weight: bold"">{onlineClass.ClassType.Name} online class</span>, scheduled to start at <span style=""font-weight: bold"">{onlineClass.DateStart.ToLongDateString()}</span> and finishing at <span style=""font-weight: bold"">{onlineClass.DateEnd.ToLongDateString()}</span> on <span style=""font-weight: bold"">{onlineClass.Platform}</span>.", "Check our other available classes");
                    var title = "Online class registration";
                    Response response = await _mailHelper.SendEmailAsync(client.Email, $"{title}", body, null, null);

                    return Ok(new { Message = "Registration for online class successfull!" });
                }
            }
            else
            {
                var gymClass = await _classRepository.GetGymClassByIdInclude(classId);

                if (gymClass == null)
                {
                    return NotFound("Gym class not found");

                }

                if (gymClass.Clients.Count == gymClass.Capacity)
                {
                    return BadRequest(new { ErrorMessage = "Couldn't register because class is already at max capacity" });
                }

                if (memberShipDetailClient.DateRenewal <= gymClass.DateStart)
                {
                    return BadRequest(new { ErrorMessage = "Class starts after memberhip's date of renewal" });
                }

                if (!gymClass.Clients.Any(c => c.Id == client.Id))
                {
                    gymClass.Clients.Add(client);
                    await _classRepository.UpdateAsync(gymClass);
                    await _registeredInClassesHistoryRepository.CreateAsync(history);

                    var body = _mailHelper.GetEmailTemplate($"Registered in {gymClass.ClassType.Name} Class", @$"Hey, {client.FirstName}, you registered yourself in the <span style=""font-weight: bold"">{gymClass.ClassType.Name} class</span>, scheduled to start at <span style=""font-weight: bold"">{gymClass.DateStart.ToLongDateString()}</span> and finishing at <span style=""font-weight: bold"">{gymClass.DateEnd.ToLongDateString()}</span> at <span style=""font-weight: bold"">{gymClass.Gym.Data}</span>.", @$"Check our other <a href=""{classesUrl}"">available classes</a>");
                    var title = "Online class registration";
                    Response response = await _mailHelper.SendEmailAsync(client.Email, $"{title}", body, null, null);

                    return Ok(new { Message = "Registration for gym class successfull!" });
                }
            }

            return BadRequest(new { ErrorMessage = "Couldn't register for class.." });
        }

        [HttpGet("[action]")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MyClasses()
        {
            var clientEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var client = await _userHelper.GetUserByEmailAsync(clientEmail) as Client;
            if (client == null)
            {
                return NotFound(new { ErrorMessage = "User not found" });
            }

            var gymClasses = await _classRepository.GetAllGymClassesInclude();
            var onlineClasses = await _classRepository.GetAllOnlineClassesInclude();

            var clientClassesModel = new List<ClassModel>();

            foreach (var gymClass in gymClasses)
            {
                if (gymClass.Clients.Any(c => c.Id == client.Id))
                {
                    clientClassesModel.Add(new ClassModel
                    {
                        Id = gymClass.Id,
                        Category = gymClass.Category.Name,
                        ClassType = gymClass.ClassType.Name,
                        Instructor = gymClass.Instructor.FullName,
                        DateStart = gymClass.DateStart,
                        DateEnd = gymClass.DateEnd,
                        Location = gymClass.Gym.Data,
                    });
                }
            }

            foreach (var onlineClass in onlineClasses)
            {
                if (onlineClass.Clients.Any(c => c.Id == client.Id))
                {
                    clientClassesModel.Add(new ClassModel
                    {
                        Id = onlineClass.Id,
                        Category = onlineClass.Category.Name,
                        ClassType = onlineClass.ClassType.Name,
                        Instructor = onlineClass.Instructor.FullName,
                        DateStart = onlineClass.DateStart,
                        DateEnd = onlineClass.DateEnd,
                        Location = onlineClass.Platform,
                    });
                }
            }

            if (clientClassesModel == null || clientClassesModel.Count == 0)
            {
                return Ok(new { Message = "Client is not registered in any classes" });
            }

            return Ok(clientClassesModel.Where(c => c.DateStart > DateTime.UtcNow).OrderBy(c => c.DateStart));
        }

        [HttpGet("[action]")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MyClassesHistory()
        {
            var clientEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var client = await _userHelper.GetUserByEmailAsync(clientEmail) as Client;
            if (client == null)
            {
                return NotFound(new { ErrorMessage = "User not found" });
            }

            var records = await _registeredInClassesHistoryRepository.GetAll().Where(r => r.UserId == client.Id && r.Canceled == false).ToListAsync();
            if (records == null)
            {
                return NotFound(new { ErrorMessage = "Client has no classes records" });
            }

            var model = new List<RegisteredInClassHistoryModel>();

            foreach (var r in records)
            {
                var classHistory = await _classHistoryRepository.GetByIdAsync(r.ClassId);
                if (classHistory == null)
                {
                    return NotFound(new { ErrorMessage = "Class history not found" });
                }

                var gym = await _gymHistoryRepository.GetByIdAsync(classHistory.GymId.Value);
                if (gym == null)
                {
                    return NotFound(new { ErrorMessage = "Gym history not found" });
                }

                var instructor = await _staffHistoryRepository.GetByStaffIdAndGymIdTrackAsync(classHistory.InstructorId, gym.Id);
                if (instructor == null)
                {
                    return NotFound(new { ErrorMessage = "Instructor history not found" });
                }

                model.Add(new RegisteredInClassHistoryModel
                {
                    Location = classHistory.GymName == "N/A" ? "Online" : classHistory.GymName,
                    CategoryName = classHistory.Category,
                    TypeName = classHistory.ClassType,
                    InstructorFullName = $"{instructor.FirstName} {instructor.LastName}",
                    StartDate = classHistory.DateStart.Value,
                    EndDate = classHistory.DateEnd.Value,
                });
            }

            model = model.Where(r => r.EndDate < DateTime.UtcNow).ToList();

            if (model == null || model.Count == 0)
            {
                return Ok(new { Message = "Client has no classes in history" });
            }

            return Ok(model);
        }
    }
}
