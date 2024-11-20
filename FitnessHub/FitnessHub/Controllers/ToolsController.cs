using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto;
using Syncfusion.EJ2.Navigations;

namespace FitnessHub.Controllers
{
    public class ToolsController : Controller
    {
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
            if(string.IsNullOrWhiteSpace(height.ToString()))
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
            if(string.IsNullOrWhiteSpace(age.ToString()))
            {
                ModelState.AddModelError(string.Empty, "Please enter a value for age");
                return View();
            }
            if (string.IsNullOrWhiteSpace(weight.ToString()))
            {
                ModelState.AddModelError(string.Empty, "Please enter a value for weight");
                return View();
            }

            if( age <= 0)
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
            var userHeight = height ;
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

    }
}
