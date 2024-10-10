﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessHub.Data.Entities;
using FitnessHub.Data.HelperClasses;
using FitnessHub.Data.Repositories;
using FitnessHub.Services;
using FitnessHub.Models;

namespace FitnessHub.Controllers
{
    public class GymsController : Controller
    {
        private readonly IGymRepository _gymRepository;
        private readonly CountryService _countryService;

        public GymsController(IGymRepository gymRepository, CountryService countryService)
        {
            _gymRepository = gymRepository;
            _countryService = countryService;
        }

        // GET: Gyms
        public IActionResult Index()
        {
            return View(_gymRepository.GetAll());
        }

        // GET: Gyms/Details/5
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
            if (ModelState.IsValid)
            {
                await _gymRepository.CreateAsync(gym);
                return RedirectToAction(nameof(Index));
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

        public IActionResult GymNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Gym not found", Message = "With so many worldwide, how did you miss this one?" });
        }
    }
}