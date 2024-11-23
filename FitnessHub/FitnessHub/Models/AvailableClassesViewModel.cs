using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessHub.Models
{
    public class AvailableClassesViewModel
    {
        public List<ClassViewModel> Classes { get; set; } = new List<ClassViewModel>();

        public List<SelectListItem> LocationList { get; set; } = new List<SelectListItem> { new SelectListItem { Value = 0.ToString(), Text = "Online Classes" } };

        // public int LocationId

        // selectlistitem para as categorias

        // public int categoryId

        // DateTime 

        // if class.DateStart.Date == DateFilter
    }
}


