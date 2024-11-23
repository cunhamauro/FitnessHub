using FitnessHub.Data.Repositories;
using FitnessHub.Models.API;
using Microsoft.AspNetCore.Mvc;

namespace FitnessHub.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassesApiController : ControllerBase
    {
        private readonly IClassRepository _classRepository;

        public ClassesApiController(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetClassesByGym(int gymId)
        {
            try
            {
                var gymClasses = await _classRepository.GetClassesByGym(gymId);

                var gymClassesModel = gymClasses.Select(c => new ClassModel
                {
                    Id = c.Id,
                    Category = c.Category.Name,
                    ClassType = c.ClassType.Name,
                    Location = c.Gym.Data,
                    Instructor = c.Instructor.FullName,
                    DateEnd = c.DateEnd,
                    DateStart = c.DateStart,
                    IsOnline = false
                });

                if (gymClassesModel == null || gymClassesModel.Count() == 0)
                {
                    return Ok(new { Message = "No classes found for that gym" });
                }

                return Ok(gymClassesModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOnlineClasses()
        {
            try
            {
                var onlineClasses = await _classRepository.GetAllOnlineClassesInclude();

                var onlineClassesModel = onlineClasses.Select(c => new ClassModel
                {

                    Id = c.Id,
                    Category = c.Category.Name,
                    ClassType = c.ClassType.Name,
                    Instructor = c.Instructor.FullName,
                    DateEnd = c.DateEnd,
                    DateStart = c.DateStart,
                    Location = c.Platform,
                    IsOnline = true
                });

                if (onlineClassesModel == null || onlineClassesModel.Count() == 0)
                {
                    return Ok(new { Message = "No online classes found" });
                }

                return Ok(onlineClassesModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
