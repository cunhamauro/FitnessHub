﻿using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Mvc;

namespace FitnessHub.Controllers
{
    public class RegisterClassesController : Controller
    {

        private readonly IClassRepository _classRepository;
        private readonly IUserHelper _userHelper;

        public RegisterClassesController(IClassRepository classRepository,
                                  IUserHelper userHelper)
        {
            _classRepository = classRepository;
            _userHelper = userHelper;
        }

        // User side actions
        public async Task<IActionResult> AvailableClasses()
        {
            var client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return NotFound();
            }

            var gymClasses = await _classRepository.GetAllGymClassesInclude();
            var onlineClasses = await _classRepository.GetAllOnlineClassesInclude();

            var viewModel = new AvailableClassesViewModel
            {
                Classes = new List<ClassViewModel>()
            };

            foreach (var gymClass in gymClasses)
            {
                if (!gymClass.Clients.Any(c => c.Id == client.Id))
                {
                    viewModel.Classes.Add(new ClassViewModel
                    {
                        InstructorName = gymClass.Instructor.FullName,
                        DateStart = gymClass.DateStart,
                        DateEnd = gymClass.DateEnd,
                        Location = gymClass.Gym.Name,
                        IsOnline = false,
                        Id = gymClass.Id,
                        Category = gymClass.Category.Name
                    });
                }
            }

            foreach (var onlineClass in onlineClasses)
            {
                if (!onlineClass.Clients.Any(c => c.Id == client.Id))
                {
                    viewModel.Classes.Add(new ClassViewModel
                    {
                        InstructorName = onlineClass.Instructor.FullName,
                        DateStart = onlineClass.DateStart,
                        DateEnd = onlineClass.DateEnd,
                        Location = "Online",
                        IsOnline = true,
                        Id = onlineClass.Id,
                        Category = onlineClass.Category.Name
                    });
                }
            }
            viewModel.Classes = viewModel.Classes.OrderBy(c => c.DateStart).ToList();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Register(int classId, bool isOnline)
        {
            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return NotFound();
            }

            if (isOnline)
            {
                var onlineClass = await _classRepository.GetOnlineClassByIdInclude(classId);

                if (onlineClass == null)
                {
                    return NotFound();
                }

                if (!onlineClass.Clients.Any(c => c.Id == client.Id))
                {
                    onlineClass.Clients.Add(client);
                    await _classRepository.UpdateAsync(onlineClass);
                }
            }
            else
            {
                var gymClass = await _classRepository.GetGymClassByIdIncludeTracked(classId);
                if (gymClass == null)
                {
                    return NotFound();
                }

                if (!gymClass.Clients.Any(c => c.Id == client.Id))
                {
                    gymClass.Clients.Add(client);
                    await _classRepository.UpdateAsync(gymClass);
                }
            }
            return RedirectToAction(nameof(AvailableClasses));
        }

        public async Task<IActionResult> MyClasses()
        {
            var client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return NotFound();
            }

            var gymClasses = await _classRepository.GetAllGymClassesInclude();
            var onlineClasses = await _classRepository.GetAllOnlineClassesInclude();

            var viewModel = new AvailableClassesViewModel
            {
                Classes = new List<ClassViewModel>()
            };

            foreach (var gymClass in gymClasses)
            {
                if (gymClass.Clients.Any(c => c.Id == client.Id))
                {
                    viewModel.Classes.Add(new ClassViewModel
                    {
                        InstructorName = gymClass.Instructor.FullName,
                        DateStart = gymClass.DateStart,
                        DateEnd = gymClass.DateEnd,
                        Location = gymClass.Gym.Name,
                        IsOnline = false,
                        Id = gymClass.Id,
                        Category = gymClass.Category.Name
                    });
                }
            }

            foreach (var onlineClass in onlineClasses)
            {
                if (onlineClass.Clients.Any(c => c.Id == client.Id))
                {
                    viewModel.Classes.Add(new ClassViewModel
                    {
                        InstructorName = onlineClass.Instructor.FullName,
                        DateStart = onlineClass.DateStart,
                        DateEnd = onlineClass.DateEnd,
                        Location = "Online",
                        IsOnline = true,
                        Id = onlineClass.Id,
                        Category = onlineClass.Category.Name
                    });
                }
            }
            viewModel.Classes = viewModel.Classes.OrderBy(c => c.DateStart).ToList();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Unregister(int id)
        {
            var client = await _userHelper.GetUserAsync(User) as Client;
            if (client == null)
            {
                return NotFound();
            }

            var gymClass = await _classRepository.GetGymClassByIdIncludeTracked(id);
            if (gymClass != null && gymClass.Clients.Contains(client))
            {
                gymClass.Clients.Remove(client);
                await _classRepository.UpdateAsync(gymClass);
                return RedirectToAction(nameof(MyClasses));
            }

            var onlineClass = await _classRepository.GetOnlineClassByIdInclude(id);
            if (onlineClass != null && onlineClass.Clients.Contains(client))
            {
                onlineClass.Clients.Remove(client);
                await _classRepository.UpdateAsync(onlineClass);
                return RedirectToAction(nameof(MyClasses));
            }
            return View();
        }

        public async Task<IActionResult> ClassDetails(int id)
        {
            var gymClass = await _classRepository.GetGymClassByIdInclude(id);
            if (gymClass != null)
            {
                var viewModel = new ClassesDetailsViewModel
                {
                    InstructorName = gymClass.Instructor.FullName,
                    DateStart = gymClass.DateStart,
                    DateEnd = gymClass.DateEnd,
                    Location = gymClass.Gym?.Name ?? "N/A",
                    Category = gymClass.Category.Name,
                    GymName = gymClass.Gym?.Name
                };
                return View(viewModel);
            }

            var onlineClass = await _classRepository.GetOnlineClassByIdInclude(id);
            if (onlineClass != null)
            {
                var viewModel = new ClassesDetailsViewModel
                {
                    InstructorName = onlineClass.Instructor.FullName,
                    DateStart = onlineClass.DateStart,
                    DateEnd = onlineClass.DateEnd,
                    Location = "Online",
                    Category = onlineClass.Category.Name,
                    Platform = onlineClass.Platform
                };
                return View(viewModel);
            }
            return NotFound();
        }



        //Employee side actions
        public async Task<IActionResult> RegisterClientInClass()
        {
            var classes = await _classRepository.GetAllGymClassesInclude();

            var model = new RegisterClientInClassViewModel
            {
                ClientEmail = "",
                Classes = classes.Select(c => new ClassDetailsViewModel
                {
                    Id = c.Id,
                    Category = c.Category.Name,
                    InstructorName = c.Instructor.FullName,
                    DateStart = c.DateStart,
                    DateEnd = c.DateEnd,
                    Location = c.Gym.Name,
                    IsClientRegistered = false
                }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterClientInClass(RegisterClientInClassViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Classes = await LoadClassDetails();
                return View(model);
            }

            var client = await _userHelper.GetUserByEmailAsync(model.ClientEmail) as Client;
            if (client == null)
            {
                ModelState.AddModelError(string.Empty, "Client not found.");
                model.Classes = await LoadClassDetails();
                return View(model);
            }

            model.IsEmailValid = true;
            model.Classes = await LoadClassDetails();

            foreach (var classItem in model.Classes)
            {
                var gymClass = await _classRepository.GetGymClassByIdInclude(classItem.Id);
                classItem.IsClientRegistered = gymClass.Clients.Any(c => c.Id == client.Id);
            }
            return View(model);
        }
        [HttpPost]

        public async Task<IActionResult> RegisterClientInClassConfirm(RegisterClientInClassViewModel model)
        {

            var client = await _userHelper.GetUserByEmailAsync(model.ClientEmail) as Client;
            if (client == null)
            {
                ModelState.AddModelError(string.Empty, "Client not found.");
                model.Classes = await LoadClassDetails();
                return View("RegisterClientInClass", model);
            }

            var allClasses = await _classRepository.GetAllGymClassesInclude();

            foreach (var gymClass in allClasses)
            {
                if (model.SelectedClassIds.Contains(gymClass.Id))
                {
                    if (!gymClass.Clients.Any(c => c.Id == client.Id))
                    {
                        gymClass.Clients.Add(client);
                    }
                }
                else
                {
                    if (gymClass.Clients.Any(c => c.Id == client.Id))
                    {
                        gymClass.Clients.Remove(client);
                    }
                }
                await _classRepository.UpdateAsync(gymClass);
            }
            return RedirectToAction(nameof(RegisterClientInClass));
        }

        private async Task<List<ClassDetailsViewModel>> LoadClassDetails()
        {
            var classes = await _classRepository.GetAllGymClassesInclude();
            return classes.Select(c => new ClassDetailsViewModel
            {
                Id = c.Id,
                Category = c.Category.Name,
                InstructorName = c.Instructor.FullName,
                DateStart = c.DateStart,
                DateEnd = c.DateEnd,
                Location = c.Gym.Name,
            }).ToList();
        }
    }
}


