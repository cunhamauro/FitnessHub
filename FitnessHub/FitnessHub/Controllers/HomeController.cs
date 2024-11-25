using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IClassHistoryRepository _classHistoryRepository;
        private readonly IClassTypeRepository _classTypeRepository;
        private readonly IMembershipRepository _membershipRepository;

        public HomeController(
            ILogger<HomeController> logger,
            IUserHelper userHelper,
            IMembershipDetailsRepository membershipDetailsRepository,
            IGymRepository gymRepository,
            IClassRepository classRepository,
            IRegisteredInClassesHistoryRepository registeredInClassesHistoryRepository,
            IMailHelper mailHelper,
            IConfiguration configuration,
            IClassHistoryRepository classHistoryRepository,
            IClassTypeRepository classTypeRepository,
            IMembershipRepository membershipRepository)
        {
            _logger = logger;
            _userHelper = userHelper;
            _membershipDetailsRepository = membershipDetailsRepository;
            _gymRepository = gymRepository;
            _classRepository = classRepository;
            _registeredInClassesHistoryRepository = registeredInClassesHistoryRepository;
            _mailHelper = mailHelper;
            _configuration = configuration;
            _classHistoryRepository = classHistoryRepository;
            _classTypeRepository = classTypeRepository;
            _membershipRepository = membershipRepository;
        }

        [Authorize(Roles = "MasterAdmin")]
        public async Task<IActionResult> Dashboard()
        {
            var model = new DashboardViewModel()
            {
                ClientsCount = (await _userHelper.GetUsersByTypeAsync<Client>()).Count,
                ClientsWithMembershipCount = await _userHelper.ClientsWithMembershipCountAsync(),
                AnnualMembershipsRevenue = await _membershipDetailsRepository.GetAnualMembershipsRevenueAsync(),
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

        public async Task<IActionResult> Index()
        {
            var gym = new Gym();

            if (this.User.Identity.IsAuthenticated && this.User.IsInRole("Client"))
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name) as Client;

                //Nao sei se fica bem estes not founds
                if (user == null)
                {
                    return UserNotFound();
                }

                if(user.MembershipDetailsId == null)
                {
                    ViewBag.ShowSignUp = true;
                }

                gym = await _gymRepository.GetGymByUserAsync(user);
                if (gym == null)
                {
                    return GymNotFound();
                }

                // Meti um codigo aqui so para verificar se tem membership para customizar a mensagem das memberships

                bool hasMembership = true;

                if (user.MembershipDetailsId == null)
                {
                    hasMembership = false;
                }

                var memberShipDetailClient = new MembershipDetails();

                if (hasMembership)
                {
                    memberShipDetailClient = await _membershipDetailsRepository.GetByIdAsync(user.MembershipDetailsId.Value);
                }

                if (memberShipDetailClient == null)
                {
                    hasMembership = false;
                }

                if (memberShipDetailClient.Status == false)
                {
                    hasMembership = false;
                }

                if (hasMembership)
                {
                    ViewBag.HasMembership = true;
                }

                ViewBag.Gym = gym;

                var users = await _userHelper.GetInstructorsByGymAsync(gym.Id);

                List<Instructor> instructors = new List<Instructor>();
                foreach (var userr in users)
                {
                    instructors.Add(userr as Instructor);
                }
                var instructorBestRating = instructors.OrderByDescending(i => i.Rating).FirstOrDefault();

                if (instructorBestRating != null)
                {
                    ViewBag.Instructor = instructorBestRating;
                }
                else
                {
                    var instructor = instructors.OrderBy(i => Guid.NewGuid()).FirstOrDefault();
                    ViewBag.Instructor = instructor;
                }

                var historyClasses = _registeredInClassesHistoryRepository.GetAll().Where(h => h.UserId == user.Id).ToList();

                // Obter todos os ClassHistories para os ClassIds do histórico
                var classIds = historyClasses.Select(h => h.ClassId).Distinct().ToList();
                var classHistories = _classHistoryRepository.GetAll()
                    .Where(ch => classIds.Contains(ch.Id))
                    .ToList();

                // Obter todos os ClassTypes correspondentes aos ClassHistories
                var classTypeNames = classHistories
                    .Where(ch => ch.ClassType != null)
                    .Select(ch => ch.ClassType)
                    .Distinct()
                    .ToList();

                // Obter os objetos ClassType correspondentes
                var classTypes = _classTypeRepository.GetAll()
                    .Where(ct => classTypeNames.Contains(ct.Name))
                    .ToList();

                // Determinar o ClassType mais frequente baseado nos nomes no histórico
                var mostFrequentClassType = classHistories
                    .Where(ch => ch.ClassType != null)
                    .GroupBy(ch => classTypes.FirstOrDefault(ct => ct.Name == ch.ClassType))
                    .Where(g => g.Key != null) // Excluir nulos
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault();

                ViewBag.RecommendClient = true;
                ViewBag.ClientFirstName = user.FirstName;

                if (mostFrequentClassType != null)
                {
                    ViewBag.Class = mostFrequentClassType;
                    ViewBag.HasClasses = true;
                }
                else
                {
                    var recommendedClass = await _classTypeRepository.GetAll().OrderBy(c => Guid.NewGuid()).FirstOrDefaultAsync();
                    ViewBag.Class = recommendedClass;
                }

                ViewBag.Welcome = $"Welcome, {user.FirstName}";
            }
            else if(this.User.Identity.IsAuthenticated && !this.User.IsInRole("Client"))
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

                ViewBag.Welcome = $"Welcome, {user.FirstName}";
                ViewBag.IsStaff = true;
            }
            else
            {
                ViewBag.Welcome = "Welcome";

                gym = await _gymRepository.GetAll().OrderBy(g => Guid.NewGuid()).FirstOrDefaultAsync();

                ViewBag.Gym = gym;

                var recommendedClass = await _classTypeRepository.GetAll().OrderBy(c => Guid.NewGuid()).FirstOrDefaultAsync();
                ViewBag.Class = recommendedClass;

                Instructor instructor = null;

                if (gym != null)
                {
                    var instructors = await _userHelper.GetInstructorsByGymAsync(gym.Id);
                    instructor = instructors.OrderBy(i => Guid.NewGuid()).FirstOrDefault() as Instructor;
                }

                ViewBag.Instructor = instructor;
            }

            return View(_membershipRepository.GetAll().Where(m => m.OnOffer == true));
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

                var response = await _mailHelper.SendEmailAsync(sender, $"Message from {name}", body, null, null);

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

        public IActionResult GymNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Gym not found", Message = "With so many worldwide, how did you miss this one?" });
        }

        public IActionResult UserNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "User not found", Message = "Looks like this user skipped leg day!" });
        }
    }
}
