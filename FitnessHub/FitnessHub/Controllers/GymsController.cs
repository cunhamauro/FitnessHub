using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessHub.Data.Entities;
using FitnessHub.Data.HelperClasses;
using FitnessHub.Data.Repositories;
using FitnessHub.Services;
using FitnessHub.Models;
using Microsoft.AspNetCore.Authorization;
using FitnessHub.Data.Entities.History;
using FitnessHub.Helpers;
using FitnessHub.Data.Entities.Users;

namespace FitnessHub.Controllers
{
    [Authorize(Roles = "MasterAdmin")]
    public class GymsController : Controller
    {
        private readonly IGymRepository _gymRepository;
        private readonly CountryService _countryService;
        private readonly IGymHistoryRepository _gymHistoryRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserHelper _userHelper;

        public GymsController(IGymRepository gymRepository, CountryService countryService, IGymHistoryRepository gymHistoryRepository, IConfiguration configuration,  IUserHelper userHelper)

        {
            _gymRepository = gymRepository;
            _countryService = countryService;
            _gymHistoryRepository = gymHistoryRepository;
            _configuration = configuration;
            _userHelper = userHelper;

        }

        // GET: Gyms
        public IActionResult Index()
        {
            return View(_gymRepository.GetAll());
        }

        // GET: Gyms/Details/5
        [Authorize(Roles = "Client, Admin, MasterAdmin, Instructor, Employee")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return GymNotFound();
            }

            var gym = await _gymRepository.GetByIdAsync(id.Value);
            if (gym == null)
            {
                return GymNotFound();
            }

            ViewBag.GoogleMapsKey = _configuration["GoogleMapsApi:Key"];


            return View(gym);
        }

        // GET: Gyms/Create
        public async Task<IActionResult> Create()
        {
            var countriesResult = await _countryService.GetCountriesAsync();
            var countries = (IEnumerable<CountryApi>)countriesResult.Results;

            ViewBag.Countries = new SelectList(countries, "Name", "Name");

            ViewBag.Cities = new SelectList(new List<string>());

            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetCitiesByCountry(string country)
        {
            var cities = await _gymRepository.GetCitiesByCountryAsync(country);

            return Json(cities);
        }

        // POST: Gyms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Gym gym)
        {
            if (gym.Country == "0")
            {
                ModelState.AddModelError("Country", "Please select a valid Country");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _gymRepository.CreateAsync(gym);

                    var gymHistory = new GymHistory()
                    {
                        Id = gym.Id,
                        Name = gym.Name,
                        Country = gym.Country,
                        City = gym.City,
                        Address = gym.Address,
                    };

                    await _gymHistoryRepository.CreateAsync(gymHistory);

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var countriesResponse = await _countryService.GetCountriesAsync();
            var countries = (IEnumerable<CountryApi>)countriesResponse.Results;

            ViewBag.Countries = new SelectList(countries, "Name", "Name");

            ViewBag.Cities = new SelectList(new List<string>());

            return View(gym);
        }

        // GET: Gyms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return GymNotFound();
            }

            var gym = await _gymRepository.GetByIdAsync(id.Value);
            if (gym == null)
            {
                return GymNotFound();
            }

            return View(gym);
        }

        // POST: Gyms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Gym gym)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _gymRepository.UpdateAsync(gym);

                    var gymHistory = await _gymHistoryRepository.GetByIdTrackAsync(gym.Id);
                    if (gymHistory == null)
                    {
                        return GymNotFound();
                    }

                    gymHistory.Id = gym.Id;
                    gymHistory.Name = gym.Name;
                    gymHistory.Country = gym.Country;
                    gymHistory.City = gym.City;
                    gymHistory.Address = gym.Address;

                    await _gymHistoryRepository.UpdateAsync(gymHistory);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _gymRepository.ExistsAsync(gym.Id))
                    {
                        return GymNotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(gym);
        }

        // GET: Gyms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return GymNotFound();
            }

            var gym = await _gymRepository.GetByIdAsync(id.Value);
            if (gym == null)
            {
                return GymNotFound();
            }

            return View(gym);
        }

        // POST: Gyms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gym = await _gymRepository.GetByIdAsync(id);
            if (gym != null)
            {

                var users = await _userHelper.GetEmployeesAndInstructorsAndClientsByGymAsync(id);

                if (users.Any())
                {
                    ModelState.AddModelError(string.Empty, "Cannot delete this gym because there are users associated with it.");
                    return View("Details", gym);
                }

                var admins = await _userHelper.GetAdminsAsync();

                var adminsByGym = admins.OfType<Admin>().Where(a => a.GymId == id);

                if (adminsByGym.Any())
                {
                    ModelState.AddModelError(string.Empty, "Cannot delete this gym because there is an admin associated with it.");
                    return View("Details", gym);
                }

                try
                {
                    await _gymRepository.DeleteAsync(gym);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    throw;
                }
            }

            return GymNotFound();
        }

        [Authorize(Roles = "Client, Admin, MasterAdmin, Instructor, Employee")]
        [AllowAnonymous]
        public IActionResult Available()
        {
            return View(_gymRepository.GetAll());
        }

        public IActionResult GymsHistory()
        {
            return View(_gymHistoryRepository.GetAll());
        }

        public IActionResult GymNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Gym not found", Message = "With so many worldwide, how did you miss this one?" });
        }
    }
}