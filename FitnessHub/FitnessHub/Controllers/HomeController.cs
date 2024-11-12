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

        public HomeController(
            ILogger<HomeController> logger, 
            IUserHelper userHelper, 
            IMembershipDetailsRepository membershipDetailsRepository,
            IGymRepository gymRepository,
            IClassRepository classRepository)
        {
            _logger = logger;
            _userHelper = userHelper;
            _membershipDetailsRepository = membershipDetailsRepository;
            _gymRepository = gymRepository;
            _classRepository = classRepository;
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
                //MostPopularClass = ,
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

        public IActionResult Contacts()
        {
            return View();
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
