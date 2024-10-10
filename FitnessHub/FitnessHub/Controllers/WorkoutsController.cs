using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Controllers
{
    public class WorkoutsController : Controller
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUserHelper _userHelper;
        private readonly IMachineRepository _machineRepository;

        public WorkoutsController(IWorkoutRepository workoutRepository, IUserHelper userHelper, IMachineRepository machineRepository)
        {
            _workoutRepository = workoutRepository;
            _userHelper = userHelper;
            _machineRepository = machineRepository;
        }

        // GET: Workouts
        public async Task<IActionResult> Index()
        {
            var workouts = await _workoutRepository.GetAllWorkouts();

            var model = workouts.Select(w => new WorkoutViewModel
            {
                Id = w.Id,
                ClientEmail = w.Client.Email,
                InstructorFullName = $"{w.Instructor.FirstName} {w.Instructor.LastName}",
            }).ToList();

            return View(model);
        }

        // GET: Workouts/Create
        public async Task<IActionResult> Create()
        {
            var model = new WorkoutViewModel
            {
                Machines = await _machineRepository.GetAllMachinesAsync()
            };

            return View(model);
        }

        // POST: Workouts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkoutViewModel model, List<Exercise> exercises)
        {
            //var instructor = await _userHelper.GetUserAsync(this.User) as Instructor;

            var client = await _userHelper.GetUserByEmailAsync(model.ClientEmail) as Client;

            if (client == null)
            {
                ModelState.AddModelError("ClientEmail", "Client not found.");
                return View(model);
            }

            //if (instructor == null)
            //{
            //    ModelState.AddModelError("InstructorId", "Instructor not found.");
            //    return View(model);
            //}

            

            if (ModelState.IsValid)
            {
                var workout = new Workout
                {
                    Client = client,
                    //Instructor = instructor,
                    Exercise = new List<Exercise>()
                };

                foreach (var exercise in exercises)
                {
                    var machine = await _machineRepository.GetByIdTrackAsync(exercise.Machine.Id);

                    workout.Exercise.Add(new Exercise
                    {
                        Machine = machine,
                        Ticks = exercise.Ticks,
                        Repetitions = exercise.Repetitions,
                        Sets = exercise.Sets,
                        DayOfWeek = exercise.DayOfWeek
                    });
                }

                await _workoutRepository.CreateAsync(workout);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workout = await _workoutRepository.GetByIdAsync(id.Value);
            if (workout == null)
            {
                return NotFound();
            }

            var model = new WorkoutViewModel
            {
                Id = workout.Id,
                ClientEmail = workout.Client.Email,
                InstructorFullName = $"{workout.Instructor.FirstName} {workout.Instructor.LastName}",
                //Exercises = workout.Exercise
            };

            return View(model);
        }

        // POST: Workouts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WorkoutViewModel model, List<Exercise> exercises)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var workout = await _workoutRepository.GetByIdAsync(id);
            if (workout == null)
            {
                return NotFound();
            }

            var client = await _userHelper.GetUserByEmailAsync(model.ClientEmail) as Client;
            if (client == null)
            {
                ModelState.AddModelError("ClientEmail", "Client not found.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                workout.Client = client;
                workout.Exercise = exercises;

                await _workoutRepository.UpdateAsync(workout);
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {

            var workout = await _workoutRepository.GetByIdAsync(id);

            if (workout == null)
            {
                return NotFound();
            }

            var model = new WorkoutViewModel
            {
                Id = workout.Id,
                ClientEmail = workout.Client.Email,
                InstructorFullName = $"{workout.Instructor.FirstName} {workout.Instructor.LastName}",
                //Exercises = workout.Exercise
            };
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workout = await _workoutRepository.GetByIdAsync(id.Value);
            if (workout == null)
            {
                return NotFound();
            }

            return View(workout);
        }

        // POST: Workouts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workout = await _workoutRepository.GetByIdAsync(id);
            if (workout == null)
            {
                return NotFound();
            }

            await _workoutRepository.DeleteAsync(workout);
            return RedirectToAction(nameof(Index));
        }
    }
}
    

    

