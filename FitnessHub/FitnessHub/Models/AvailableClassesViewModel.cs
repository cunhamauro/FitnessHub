using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessHub.Models
{
    public class AvailableClassesViewModel
    {
        public List<ClassViewModel> Classes { get; set; } = new List<ClassViewModel>();
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public int LocationId { get; set; }
        public int CategoryId { get; set; }
        public DateTime? DateFilter { get; set; }

        // public int LocationId

        // selectlistitem para as categorias

        // public int categoryId

        // DateTime 

        // if class.DateStart.Date == DateFilter
    }
}


