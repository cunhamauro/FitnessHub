using FitnessHub.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FitnessHub.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class GymsApiController : ControllerBase
    {
        private readonly IGymRepository _gymRepository;

        public GymsApiController(IGymRepository gymRepository)
        {
            _gymRepository = gymRepository;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAllGyms()
        {
            try
            {
                var gyms = _gymRepository.GetAll();

                if (gyms == null)
                {
                    return NotFound(new { ErrorMessage = "No gyms found" });
                }

                return Ok(gyms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
