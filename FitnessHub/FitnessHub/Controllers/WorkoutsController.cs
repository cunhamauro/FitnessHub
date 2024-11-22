using FitnessHub.Data.Classes;
using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;

namespace FitnessHub.Controllers
{
    public class WorkoutsController : Controller
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUserHelper _userHelper;
        private readonly IMachineRepository _machineRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IMailHelper _mailHelper;
        private readonly IPdfHelper _pdfHelper;

        public WorkoutsController(IWorkoutRepository workoutRepository, IUserHelper userHelper, IMachineRepository machineRepository, IExerciseRepository exerciseRepository, IMailHelper mailHelper, IPdfHelper pdfHelper)
        {
            _workoutRepository = workoutRepository;
            _userHelper = userHelper;
            _machineRepository = machineRepository;
            _exerciseRepository = exerciseRepository;
            _mailHelper = mailHelper;
            _pdfHelper = pdfHelper;
        }

        // GET: Client Workouts
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyWorkouts()
        {
            // Get current client
            var client = await _userHelper.GetUserAsync(this.User) as Client;

            List<Workout> clientWorkouts = await _workoutRepository.GetClientWorkoutsIncludeAsync(client);

            return View(clientWorkouts);
        }

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> ClientWorkoutDetails(int id)
        {
            var workout = await _workoutRepository.GetWorkoutByIdIncludeAsync(id);

            // Get current client
            var client = await _userHelper.GetUserAsync(this.User) as Client;

            if (workout.Client != client)
            {
                return WorkoutNotFound();
            }

            if (workout == null)
            {
                return WorkoutNotFound();
            }

            foreach (var exercise in workout.Exercises)
            {
                if (exercise.DayOfWeek == DayOfWeek.Monday)
                {
                    ViewBag.Monday = true;
                }
                else if (exercise.DayOfWeek == DayOfWeek.Tuesday)
                {
                    ViewBag.Tuesday = true;
                }
                else if (exercise.DayOfWeek == DayOfWeek.Wednesday)
                {
                    ViewBag.Wednesday = true;
                }
                else if (exercise.DayOfWeek == DayOfWeek.Thursday)
                {
                    ViewBag.Thursday = true;
                }
                else if (exercise.DayOfWeek == DayOfWeek.Friday)
                {
                    ViewBag.Friday = true;
                }
                else if (exercise.DayOfWeek == DayOfWeek.Saturday)
                {
                    ViewBag.Saturday = true;
                }
                else if (exercise.DayOfWeek == DayOfWeek.Sunday)
                {
                    ViewBag.Sunday = true;
                }
            }

            return View(workout);
        }

        // GET: Workouts
        [Authorize(Roles = "Instructor")]
        public IActionResult Index()
        {
            return View(_workoutRepository.GetAll().Include(w => w.Client).Include(w => w.Instructor).Include(w => w.Exercises).ThenInclude(m => m.Machine).ThenInclude(m => m.Category));
        }

        // GET: Workouts/Create
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Create()
        {
            var machines = await _machineRepository.GetAll().ToListAsync();

            var machinesDto = new List<MachineDTO>();

            foreach (var machine in machines)
            {
                machinesDto.Add(new MachineDTO
                {
                    Id = machine.Id,
                    Name = machine.Name,
                });
            }

            var model = new WorkoutViewModel
            {
                Machines = machines,
                MachinesDTO = machinesDto,
            };

            return View(model);
        }

        // POST: Workouts/Create
        [Authorize(Roles = "Instructor")]
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

            Client client = new Client();

            client = await _userHelper.GetUserByEmailAsync(model.ClientEmail) as Client;

            if (client == null)
            {
                ModelState.AddModelError("ClientEmail", "Client not found");
            }

            // Validation of the maximum duration for each exercise
            TimeSpan maxTime = new TimeSpan(23, 59, 59);

            foreach (var ex in model.Exercises)
            {
                if (ex.Time > maxTime)
                {
                    ModelState.AddModelError($"Exercises[{ex.Id}].Time", "The duration must not exceed 23H:59M:59S.");
                }
            }

            if (model.Exercises == null || !model.Exercises.Any())
            {
                return RedirectToAction("Create", "Workouts");
            }

            if (ModelState.IsValid)
            {
                Workout workout = new Workout
                {
                    DateModified = DateTime.UtcNow,
                    Client = client,
                    Instructor = instructor,
                    Exercises = new List<Exercise>(),
                };

                foreach (var exerModel in model.Exercises)
                {
                    var machine = await _machineRepository.GetByIdTrackAsync(exerModel.MachineId); // With track to nest existing machine inside new exercise < workout

                    workout.Exercises.Add(new Exercise
                    {
                        Name = exerModel.Name,
                        Machine = machine,
                        Ticks = exerModel.Time.Ticks,
                        Repetitions = exerModel.Repetitions,
                        Sets = exerModel.Sets,
                        DayOfWeek = exerModel.DayOfWeek,
                        Notes = string.IsNullOrEmpty(exerModel.Notes) ? "N/A" : exerModel.Notes,
                    });
                }

                workout.Exercises = workout.Exercises.OrderBy(exer => exer.DayOfWeek).ToList();

                await _workoutRepository.CreateAsync(workout);

                MemoryStream workoutsPdf = _pdfHelper.GenerateWorkoutPdf(client, instructor, workout);

                string body = _mailHelper.GetEmailTemplate("New Workout", @$"Hey, {client.FirstName}, check your new workout plan assigned by <span style=""font-weight: bold"">{instructor.FirstName} {instructor.LastName} [{instructor.Email}]</span>.<br/>You can find it attached here or in your FitnessHub account.", $"Enjoy your new workout");

                DateTime date = DateTime.UtcNow;

                Response response = await _mailHelper.SendEmailAsync(client.Email, "Workout Assigned", body, workoutsPdf, $"fitnesshub_workout_{client.FirstName.ToLower()}_{client.LastName.ToLower()}_{date.ToString("dMyyyy")}.pdf");

                return RedirectToAction(nameof(Index));
            }

            var machines = await _machineRepository.GetAll().ToListAsync();

            var machinesDto = new List<MachineDTO>();

            foreach (var machine in machines)
            {
                machinesDto.Add(new MachineDTO
                {
                    Id = machine.Id,
                    Name = machine.Name,
                });
            }

            model.Machines = machines;
            model.MachinesDTO = machinesDto;

            return View(model);
        }

        [Authorize(Roles = "Instructor")]
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
                    Time = exercise.Time,
                    Repetitions = exercise.Repetitions,
                    Sets = exercise.Sets,
                    DayOfWeek = exercise.DayOfWeek,
                    Notes = exercise.Notes,
                });
            }

            var machines = await _machineRepository.GetAll().ToListAsync();

            var machinesDto = new List<MachineDTO>();

            foreach (var machine in machines)
            {
                machinesDto.Add(new MachineDTO
                {
                    Id = machine.Id,
                    Name = machine.Name,
                });
            }

            var model = new WorkoutViewModel
            {
                Id = workout.Id,
                ClientEmail = workout.Client.Email,
                Instructor = workout.Instructor,
                Exercises = exercisesModel,
                Machines = machines,
                MachinesDTO = machinesDto,
            };

            return View(model);
        }

        public async Task<IActionResult> DownloadWorkoutPdf(int id)
        {
            Workout model = await _workoutRepository.GetWorkoutByIdIncludeAsync(id);
            Client client = model.Client;
            Instructor instructor = model.Instructor;

            // Generate the PDF content
            var pdfStream = _pdfHelper.GenerateWorkoutPdf(client, instructor, model);

            DateTime date = DateTime.UtcNow;

            // Return the PDF as a file response
            return File(pdfStream, "application/pdf", @$"fitnesshub_workout_{client.FirstName.ToLower()}_{client.LastName.ToLower()}_{date.ToString("dMyyyy")}.pdf");
        }

        // POST: Workouts/Edit/5
        [Authorize(Roles = "Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WorkoutViewModel model)
        {
            var workout = await _workoutRepository.GetWorkoutByIdIncludeAsync(model.Id);

            if (workout == null)
            {
                return WorkoutNotFound();
            }

            if (model.Exercises == null || !model.Exercises.Any())
            {
                return RedirectToAction("Edit", "Workouts", new { id = model.Id });
            }

            if (model.Exercises == null || !model.Exercises.Any())
            {
                ModelState.AddModelError("", "A Workout must contain at least one Exercise");
            }

            // Validation of the maximum duration for each exercise
            TimeSpan maxTime = new TimeSpan(23, 59, 59);

            foreach (var ex in model.Exercises)
            {
                if (ex.Time > maxTime)
                {
                    ModelState.AddModelError($"Exercises[{ex.Id}].Time", "The duration must not exceed 23H:59M:59S.");
                }
            }

            List<int> newIds = new();

            if (ModelState.IsValid)
            {
                // Update or add exercises
                foreach (var exerModel in model.Exercises)
                {
                    // Find existing exercise
                    var existingExercise = workout.Exercises.FirstOrDefault(e => e.Id == exerModel.Id);

                    var machine = await _machineRepository.GetByIdTrackAsync(exerModel.MachineId); // Get the exercise machine with track to nest existing machine updated workout

                    // If it exists, update it
                    if (existingExercise != null)
                    {
                        existingExercise.Name = exerModel.Name;
                        existingExercise.Machine = machine;
                        existingExercise.Ticks = exerModel.Time.Ticks;
                        existingExercise.Repetitions = exerModel.Repetitions;
                        existingExercise.Sets = exerModel.Sets;
                        existingExercise.DayOfWeek = exerModel.DayOfWeek;
                        existingExercise.Notes = exerModel.Notes;
                    }
                    else
                    {
                        // If it doesn't exist, create a new exercise
                        var newExercise = new Exercise
                        {
                            Name = exerModel.Name,
                            Machine = machine,
                            Ticks = exerModel.Time.Ticks,
                            Repetitions = exerModel.Repetitions,
                            Sets = exerModel.Sets,
                            DayOfWeek = exerModel.DayOfWeek,
                            Notes = exerModel.Notes
                        };

                        await _exerciseRepository.CreateAsync(newExercise);

                        newIds.Add(newExercise.Id);

                        workout.Exercises.Add(newExercise);
                    }
                }

                var exerciseIdsToKeep = model.Exercises.Where(e => e.Id > 0).Select(e => e.Id).ToList();

                foreach (var id in newIds)
                {
                    exerciseIdsToKeep.Add(id);
                }
                
                var exercisesToRemove = workout.Exercises.Where(e => !exerciseIdsToKeep.Contains(e.Id)).ToList();

                foreach (var exercise in exercisesToRemove)
                {
                    await _exerciseRepository.DeleteAsync(exercise);
                }

                workout.DateModified = DateTime.UtcNow;

                await _workoutRepository.UpdateAsync(workout);

                return RedirectToAction(nameof(Index));
            }

            var machines = await _machineRepository.GetAll().ToListAsync();
            var machinesDto = new List<MachineDTO>();

            foreach (var machine in machines)
            {
                machinesDto.Add(new MachineDTO
                {
                    Id = machine.Id,
                    Name = machine.Name,
                });
            }

            model.MachinesDTO = machinesDto;
            model.Machines = machines;

            return View(model);
        }

        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Details(int id)
        {
            var workout = await _workoutRepository.GetWorkoutByIdIncludeAsync(id);

            if (workout == null)
            {
                return WorkoutNotFound();
            }

            foreach (var exercise in workout.Exercises)
            {
                if (exercise.DayOfWeek == DayOfWeek.Monday)
                {
                    ViewBag.Monday = true;
                }
                else if (exercise.DayOfWeek == DayOfWeek.Tuesday)
                {
                    ViewBag.Tuesday = true;
                }
                else if (exercise.DayOfWeek == DayOfWeek.Wednesday)
                {
                    ViewBag.Wednesday = true;
                }
                else if (exercise.DayOfWeek == DayOfWeek.Thursday)
                {
                    ViewBag.Thursday = true;
                }
                else if (exercise.DayOfWeek == DayOfWeek.Friday)
                {
                    ViewBag.Friday = true;
                }
                else if (exercise.DayOfWeek == DayOfWeek.Saturday)
                {
                    ViewBag.Saturday = true;
                }
                else if (exercise.DayOfWeek == DayOfWeek.Sunday)
                {
                    ViewBag.Sunday = true;
                }
            }

            return View(workout);
        }

        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Delete(int? id)
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

            return View(workout);
        }

        // POST: Workouts/Delete/5
        [Authorize(Roles = "Instructor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workout = await _workoutRepository.GetWorkoutByIdIncludeAsync(id);
            if (workout == null)
            {
                return WorkoutNotFound();
            }

            List<Exercise> exercisesToRemove = new List<Exercise>();

            foreach (var exercise in workout.Exercises)
            {
                exercisesToRemove.Add(exercise); // Collecting exercises to delete
            }

            // Now, delete each collected exercise
            foreach (var exercise in exercisesToRemove)
            {
                await _exerciseRepository.DeleteAsync(exercise); // Deleting the exercise
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