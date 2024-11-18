using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FitnessHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserHelper _userHelper;
        private readonly IMembershipDetailsRepository _membershipDetailsRepository;
        private readonly IGymRepository _gymRepository;
        private readonly IClassRepository _classRepository;
        private readonly IRegisteredInClassesHistoryRepository _registeredInClassesHistoryRepository;
        private readonly IMailHelper _mailHelper;
        private readonly IConfiguration _configuration;

        public HomeController(
            ILogger<HomeController> logger, 
            IUserHelper userHelper, 
            IMembershipDetailsRepository membershipDetailsRepository,
            IGymRepository gymRepository,
            IClassRepository classRepository,
            IRegisteredInClassesHistoryRepository registeredInClassesHistoryRepository,
            IMailHelper mailHelper,
            IConfiguration configuration)
        {
            _logger = logger;
            _userHelper = userHelper;
            _membershipDetailsRepository = membershipDetailsRepository;
            _gymRepository = gymRepository;
            _classRepository = classRepository;
            _registeredInClassesHistoryRepository = registeredInClassesHistoryRepository;
            _mailHelper = mailHelper;
            _configuration = configuration;
        }

        [Authorize(Roles = "MasterAdmin")]
        public async Task<IActionResult> Dashboard()
        {
            var model = new DashboardViewModel()
            {
                ClientsCount = (await _userHelper.GetUsersByTypeAsync<Client>()).Count,
                ClientsWithMembershipCount = await _userHelper.ClientsWithMembershipCountAsync(),
                AnualMembershipsRevenue = await _membershipDetailsRepository.GetAnualMembershipsRevenueAsync(),
                GymWithMostMemberShips = await _userHelper.GymWithMostMembershipsAsync(),
                GymsCount = _gymRepository.GetAll().Count(), 
                EmployeesCount = (await _userHelper.GetUsersByTypeAsync<Employee>()).Count,
                InstructorsCount = (await _userHelper.GetUsersByTypeAsync<Instructor>()).Count,
                CountriesCount = await _gymRepository.GetCountriesCountAsync(),
                ScheduledGymClassesCount = (await _classRepository.GetAllGymClassesInclude()).Count,
                ScheduledOnlineClassesCount = (await _classRepository.GetAllOnlineClassesInclude()).Count,
                VideoClassesCount = (await _classRepository.GetAllVideoClassesInclude()).Count,
                MostPopularClass = await _registeredInClassesHistoryRepository.GetMostPopularClass(),
            };

            return View(model);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Contacts()
        {
            var email = "";
            var name = "";

            if (this.User.Identity.IsAuthenticated)
            {
                var user = await _userHelper.GetUserAsync(this.User);

                if (user != null)
                {
                    email = user.Email;
                    name = user.FullName;
                }
            }

            var model = new SendEmailViewModel()
            {
                Email = email,
                Name = name,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Contacts(SendEmailViewModel model)
        {
            string email = model.Email;
            string title = model.Subject;
            string message = model.Message;
            string name = model.Name;

            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("Email", "Please write a valid email");
            }

            if (string.IsNullOrEmpty(name))
            {
                ModelState.AddModelError("Name", "Please write a valid name");
            }

            if (string.IsNullOrEmpty(title))
            {
                ModelState.AddModelError("Email", "Please write a valid subject");
            }

            if (string.IsNullOrEmpty(message))
            {
                ModelState.AddModelError("Email", "Please write a valid message");
            }

            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(email);

                string footerMessage = $"{email} is not registered at FitnessHub";

                if (user != null)
                {
                    IList<string> role = await _userHelper.GetUserRolesAsync(user);
                    footerMessage = $"{email} is a FitnessHub {role.First()}";
                }

                title = $@"<span style=""font-size: 15px; color: #a9a9a9"">From: {name}&nbsp;[{email}]</span><br/>{title}";

                string body = _mailHelper.GetEmailTemplate(title, message, footerMessage);

                string sender = _configuration["Mail:SenderEmail"];

                var response = await _mailHelper.SendEmailAsync(sender, $"Message from {name}", body);

                ViewBag.ShowMessage = true;

                if (response.IsSuccess)
                {
                    model.Subject = "";
                    model.Message = "";

                    ModelState["Message"].AttemptedValue = "";

                    ViewBag.Message = "The email was successfully sent";
                    ViewBag.Color = "text-success";

                    return View(model);
                }
                else
                {
                    ViewBag.Message = "The email could not be sent. Try again";
                    ViewBag.Color = "text-danger";
                }
            }

            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult NotAuthorized()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Not authorized", Message = $"You haven't warmed up enough for this!" });
        }

        public IActionResult PageNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Page not found", Message = $"Take a sip of whey and look for it again!" });
        }
    }
}
