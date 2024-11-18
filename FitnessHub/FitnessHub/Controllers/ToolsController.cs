using Microsoft.AspNetCore.Mvc;

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
    }
}
