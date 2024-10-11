using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Mvc;

namespace FitnessHub.Controllers
{
    public class WorkoutsController : Controller
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUserHelper _userHelper;
        private readonly IMachineRepository _machineRepository;
        private readonly IExerciseRepository _exerciseRepository;

        public WorkoutsController(IWorkoutRepository workoutRepository, IUserHelper userHelper, IMachineRepository machineRepository, IExerciseRepository exerciseRepository)
        {
            _workoutRepository = workoutRepository;
            _userHelper = userHelper;
            _machineRepository = machineRepository;
            _exerciseRepository = exerciseRepository;
        }

        // GET: Workouts
        public IActionResult Index()
        {
            return View(_workoutRepository.GetAll());
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
        public async Task<IActionResult> Create(WorkoutViewModel model)
        {
            // Get current instructor
            var instructor = await _userHelper.GetUserAsync(this.User) as Instructor;

            if (instructor == null)
            {
                return UserNotFound();
            }

            var client = await _userHelper.GetUserByEmailAsync(model.ClientEmail) as Client;

            if (client == null)
            {
                ModelState.AddModelError("ClientEmail", "Client not found");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                Workout workout = new Workout
                {
                    Client = client,
                    Instructor = instructor,
                    Exercises = new List<Exercise>(),
                };

                foreach (var exerModel in model.Exercises)
                {
                    Machine machine = await _machineRepository.GetByIdTrackAsync(exerModel.MachineId); // With track to nest existing machine inside new workout

                    workout.Exercises.Add(new Exercise
                    {
                        Machine = machine,
                        Ticks = exerModel.Ticks,
                        Repetitions = exerModel.Repetitions,
                        Sets = exerModel.Sets,
                        DayOfWeek = exerModel.DayOfWeek
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
                return WorkoutNotFound();
            }

            var workout = await _workoutRepository.GetWorkoutByIdIncludeAsync(id.Value);

            if (workout == null)
            {
                return WorkoutNotFound();
            }

            var exercisesModel = new List<ExerciseViewModel>();

            foreach (Exercise exercise in workout.Exercises)
            {
                exercisesModel.Add(new ExerciseViewModel
                {
                    Id = exercise.Id,
                    MachineId = exercise.Machine.Id,
                    Name = exercise.Name,
                    Ticks = exercise.Ticks,
                    Repetitions = exercise.Repetitions,
                    Sets = exercise.Sets,
                    DayOfWeek= exercise.DayOfWeek
                });
            }

            var model = new WorkoutViewModel
            {
                Id = workout.Id,
                ClientEmail = workout.Client.Email,
                Instructor = workout.Instructor,
                Exercises =  exercisesModel,
                Machines = await _machineRepository.GetAllMachinesAsync(),
            };

            return View(model);
        }

        // POST: Workouts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WorkoutViewModel model)
        {
            var workout = await _workoutRepository.GetWorkoutByIdIncludeAsync(model.Id);

            if (workout == null)
            {
                return WorkoutNotFound();
            }

            var client = workout.Client;

            if (client == null)
            {
                ModelState.AddModelError("ClientEmail", "Client not found");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                // Update or add exercises
                foreach (var exerModel in model.Exercises)
                {
                    // Find existing exercise
                    var existingExercise = workout.Exercises.FirstOrDefault(e => e.Id == exerModel.Id);

                    Machine machine = await _machineRepository.GetByIdTrackAsync(exerModel.MachineId); // Get the exercise machine with track to nest existing machine updated workout

                    // If it exists, update it
                    if (existingExercise != null)
                    {
                        existingExercise.Machine = machine;
                        existingExercise.Ticks = exerModel.Ticks;
                        existingExercise.Repetitions = exerModel.Repetitions;
                        existingExercise.Sets = exerModel.Sets;
                        existingExercise.DayOfWeek = exerModel.DayOfWeek;
                    }
                    else
                    {
                        // If it doesn't exist, create a new exercise
                        var newExercise = new Exercise
                        {
                            Machine = machine,
                            Ticks = exerModel.Ticks,
                            Repetitions = exerModel.Repetitions,
                            Sets = exerModel.Sets,
                            DayOfWeek = exerModel.DayOfWeek
                        };

                        workout.Exercises.Add(newExercise);
                    }
                }

                var exerciseIdsToKeep = model.Exercises.Select(e => e.Id).ToList();
                var exercisesToRemove = workout.Exercises.Where(e => !exerciseIdsToKeep.Contains(e.Id)).ToList();

                foreach (var exercise in exercisesToRemove)
                {
                    await _exerciseRepository.DeleteAsync(exercise);
                }

                await _workoutRepository.UpdateAsync(workout);

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var workout = await _workoutRepository.GetWorkoutByIdIncludeAsync(id);

            if (workout == null)
            {
                return WorkoutNotFound();
            }

            return View(workout);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return WorkoutNotFound();
            }

            var workout = await _workoutRepository.GetByIdAsync(id.Value);
            if (workout == null)
            {
                return WorkoutNotFound();
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
                return WorkoutNotFound();
            }

            await _workoutRepository.DeleteAsync(workout);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult UserNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "User not found", Message = "Looks like this user skipped leg day!" });
        }

        public IActionResult WorkoutNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Workout not found", Message = "Maybe it was left behind in the locker room!" });
        }
    }
}




