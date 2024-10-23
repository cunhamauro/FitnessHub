using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessHub.Models
{
    public class MachineDetailsViewModel
    {
        public int MachineId { get; set; }

        public IEnumerable<SelectListItem>? Machines { get; set; }
    }
}
