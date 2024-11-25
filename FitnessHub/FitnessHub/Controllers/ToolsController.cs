using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.GymClasses;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;
using Syncfusion.EJ2.Navigations;
using System.Security.Claims;

namespace FitnessHub.Controllers
{
    public class ToolsController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IUserRecordWeight _userRecordWeight;
        private readonly IImageHelper _imageHelper;

        public ToolsController(IUserHelper userHelper, IUserRecordWeight userRecordWeight, IImageHelper imageHelper)
        {
            _userHelper = userHelper;
            _userRecordWeight = userRecordWeight;
            _imageHelper = imageHelper;
        }
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult CalculateImc()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CalculateImc(int height, float weight)
        {
            if (string.IsNullOrWhiteSpace(height.ToString()))
            {
                ModelState.AddModelError(string.Empty, "Please enter a value for height");
                return View();
            }

            if (string.IsNullOrWhiteSpace(weight.ToString()))
            {
                ModelState.AddModelError(string.Empty, "Please enter a value for weight");
                return View();
            }

            if (height <= 0 || weight <= 0)
            {
                ModelState.AddModelError(string.Empty, "Please enter valid positive values for height and weight.");
                return View();
            }

            var userHeight = height / 100.0;

            var userWeight = weight;

            var imc = userWeight / (userHeight * userHeight);

            ViewBag.Imc = Math.Round(imc, 2); ;

            string imcCategory = "";
            string imcColor = "";

            if (imc < 18.5)
            {
                imcCategory = "Underweight";
                imcColor = "text-yellow";
            }
            else if (imc >= 18.5 && imc < 25)
            {
                imcCategory = "Normal weight";
                imcColor = "text-success";
            }
            else if (imc >= 25 && imc < 30)
            {
                imcCategory = "Overweight";
                imcColor = "text-orange";
            }
            else
            {
                imcCategory = "Obesity";
                imcColor = "text-red";
            }

            ViewBag.ImcCategory = imcCategory;
            ViewBag.ImcColor = imcColor;

            return View();
        }

        public IActionResult CalculateWaterRequirements()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CalculateWaterRequirements(float weight)
        {
            if (string.IsNullOrWhiteSpace(weight.ToString()))
            {
                ModelState.AddModelError(string.Empty, "Please enter a value for weight");
                return View();
            }

            if (weight <= 0)
            {
                ModelState.AddModelError(string.Empty, "Please enter valid positive values for weight.");
                return View();
            }

            var userWeight = weight;

            var intakeNeeded = (weight * 35) / 1000;

            ViewBag.WaterIntake = Math.Round(intakeNeeded, 2);

            return View();
        }

        public IActionResult DailyCaloriesInTake()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DailyCaloriesInTake(int age, float weight, int height, string genre, string objective, string activityLevel)
        {
            if (string.IsNullOrWhiteSpace(age.ToString()))
            {
                ModelState.AddModelError(string.Empty, "Please enter a value for age");
                return View();
            }
            if (string.IsNullOrWhiteSpace(weight.ToString()))
            {
                ModelState.AddModelError(string.Empty, "Please enter a value for weight");
                return View();
            }

            if (age <= 0)
            {

                ModelState.AddModelError(string.Empty, "Please enter valid positive values for age.");
                return View();
            }

            if (weight <= 0)
            {
                ModelState.AddModelError(string.Empty, "Please enter valid positive values for weight.");
                return View();
            }

            if (height <= 0)
            {
                ModelState.AddModelError(string.Empty, "Please enter valid positive values for height.");
                return View();
            }

            var userAge = age;
            var userHeight = height;
            var userWeight = weight;
            var userObjective = objective;
            var userGenre = genre;

            double tmb;

            if (userGenre == "Male")
            {
                tmb = 88.36 + (13.4 * userWeight) + (4.8 * userHeight) - (5.7 * userAge);
            }
            else
            {
                tmb = 447.6 + (9.25 * userWeight) + (3.1 * userHeight) - (4.3 * userAge);
            }

            double activityFactor;

            switch (activityLevel.ToLower())
            {
                case "low":
                    activityFactor = 1.2;
                    break;
                case "moderate":
                    activityFactor = 1.55;
                    break;
                case "high":
                    activityFactor = 1.9;
                    break;
                default:

                    ModelState.AddModelError(string.Empty, "Please enter a valid activity level (low, moderate, or high).");
                    return View();
            }

            var dailyCalories = tmb * activityFactor;

            if (userObjective.ToLower() == "maintain")
            {

            }
            else if (userObjective.ToLower() == "lose weight")
            {
                dailyCalories *= 0.8;
            }
            else if (userObjective.ToLower() == "gain muscle")
            {
                dailyCalories *= 1.2;
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Please enter a valid objective (maintain, lose weight, or gain muscle).");
                return View();
            }

            ViewBag.DailyCalories = Math.Round(dailyCalories, 2);

            return View();
        }

        public async Task<IActionResult> RecordWeightIndex()
        {
            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
               return UserNotFound();
            }

            var progress = await _userRecordWeight.GetWeightProgressAsync(client.Id);

            if (progress == null || !progress.Any())
            {
                return RedirectToAction(nameof(RecordWeight));
            }

            progress = progress.OrderByDescending(p => p.Date).ToList();

            
            var sortedAscending = progress.OrderBy(p => p.Date).ToList();

            
            sortedAscending[0].Progress = 0;
            for (int i = 1; i < sortedAscending.Count; i++)
            {
                sortedAscending[i].Progress = sortedAscending[i].Weight - sortedAscending[i - 1].Weight;
            }

            
            for (int i = 0; i < progress.Count; i++)
            {
                var correspondingRecord = sortedAscending.FirstOrDefault(p => p.Id == progress[i].Id);
                if (correspondingRecord != null)
                {
                    progress[i].Progress = correspondingRecord.Progress;
                }
            }

            List<WeightLogModel> logs = new();

            foreach (var item in progress)
            {
                logs.Add(new WeightLogModel()
                {
                    Date = item.Date,
                    Weight = item.Weight,
                });
            }

            logs = logs.OrderBy(l => l.Date).ToList();

            ViewBag.WeightLog = logs;

            return View(progress);
            
        }

        public async Task<IActionResult> RecordWeight()
        {
            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
            }

            var progress = await _userRecordWeight.GetWeightProgressAsync(client.Id);

            if (progress.Any())
            {
                ViewBag.IndexExists = true;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecordWeight(WeightProgressViewModel model, float weight)
        {
            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
            }

            if (weight <= 0)
            {
                ModelState.AddModelError(string.Empty, "Please enter a valid weight.");
                var progress = await _userRecordWeight.GetWeightProgressAsync(client.Id);
                return View(progress);
            }

            var lastWeight = await _userRecordWeight.GetLastWeightProgress(client.Id);

            var weightRecord = new WeightProgress
            {
                CustomerId = client.Id,
                Weight = weight,
                Date = DateTime.Now
            };

            var path = string.Empty;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                path = await _imageHelper.UploadImageAsync(model.ImageFile, "clientProgress");
                if (!string.IsNullOrEmpty(path))
                {
                    weightRecord.ImagePath = path;
                }
            }

            if (lastWeight != null)
            {
                weightRecord.Progress = weight - lastWeight.Weight;
            }
            else
            {
                weightRecord.Progress = 0;  
            }

            await _userRecordWeight.CreateAsync(weightRecord);
            return RedirectToAction(nameof(RecordWeightIndex));
        }

        public async Task<IActionResult> DeleteRecordWeight(int? id)
        {
            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
            }

            if(id  <= 0)
            {
                return WeightHistoryNotFound();
            }

            var progress = await _userRecordWeight.GetWeightProgressAsync(client.Id);

            if (progress == null || !progress.Any())
            {
                return RedirectToAction(nameof(RecordWeight));
            }

            return View(progress);
        }

        // POST: Machines/Delete/5
        [HttpPost, ActionName("DeleteRecordWeight")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRecordWeightConfirm(int id)
        {
            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
            }

            var progress = await _userRecordWeight.GetWeightProgressAsync(client.Id);

            if (progress == null || !progress.Any())
            {
                return RedirectToAction(nameof(RecordWeightIndex));
            }
            
            foreach (var record in progress)
            {
                await _userRecordWeight.DeleteAsync(record);
            }
            return RedirectToAction(nameof(RecordWeightIndex)); 

        }

            public IActionResult WeightHistoryNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Weight history not found", Message = "Weight history not found! Start to register you progress!" });
        }

        public IActionResult UserNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "User not found", Message = "Looks like this user skipped leg day!" });
        }
    }
}
