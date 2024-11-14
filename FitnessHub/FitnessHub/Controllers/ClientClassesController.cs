﻿using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.GymClasses;
using FitnessHub.Data.Entities.History;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace FitnessHub.Controllers
{
    public class ClientClassesController : Controller
    {

        private readonly IClassRepository _classRepository;
        private readonly IUserHelper _userHelper;
        private readonly IRegisteredInClassesHistoryRepository _registeredInClassesHistoryRepository;
        private readonly IClassHistoryRepository _classHistoryRepository;
        private readonly IGymHistoryRepository _gymHistoryRepository;
        private readonly IStaffHistoryRepository _staffHistoryRepository;

        public ClientClassesController(IClassRepository classRepository,
                                  IUserHelper userHelper, IRegisteredInClassesHistoryRepository registeredInClassesHistoryRepository, IClassHistoryRepository classHistoryRepository, IGymHistoryRepository gymHistoryRepository, IStaffHistoryRepository staffHistoryRepository)
        {
            _classRepository = classRepository;
            _userHelper = userHelper;
            _registeredInClassesHistoryRepository = registeredInClassesHistoryRepository;
            _classHistoryRepository = classHistoryRepository;
            _gymHistoryRepository = gymHistoryRepository;
            _staffHistoryRepository = staffHistoryRepository;
        }

        public async Task<IActionResult> MyClassHistory()
        {
            var client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
            }

            List<RegisteredInClassesHistory> records = await _registeredInClassesHistoryRepository.GetAll().Where(c => c.UserId == client.Id).ToListAsync();

            List<RegisteredInClassesHistoryViewModel> model = new();

            foreach (var r in records)
            {

                ClassHistory gClass = await _classHistoryRepository.GetByIdAsync(r.ClassId);

                // Fetch employee and instructor if available
                StaffHistory? employee = null;
                StaffHistory? instructor = null;

                if (!string.IsNullOrEmpty(r.EmployeeId))
                {
                    employee = await _staffHistoryRepository.GetByIdTrackAsync(r.EmployeeId);
                }

                if (!string.IsNullOrEmpty(gClass.InstructorId))
                {
                    instructor = await _staffHistoryRepository.GetByIdTrackAsync(gClass.InstructorId);
                }

                model.Add(new RegisteredInClassesHistoryViewModel
                {
                    GymName = gClass.GymName,
                    CategoryName = gClass.Category,
                    TypeName = gClass.ClassType,
                    EmployeeEmail = employee?.Email ?? string.Empty,
                    EmployeeFullName = employee != null ? $"{employee.FirstName} {employee.LastName}" : string.Empty,
                    SubClass = gClass.SubClass,
                    RegistrationDate = r.RegistrationDate,
                    InstructorEmail = instructor?.Email ?? string.Empty,
                    InstructorFullName = instructor != null ? $"{instructor.FirstName} {instructor.LastName}" : string.Empty,
                });
            }
             return View(model);
        }

        // User side actions
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> AvailableClasses()
        {
            var client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
            }

            var gymClasses = await _classRepository.GetAllGymClassesInclude();
            gymClasses = gymClasses.Where(c => c.Clients.Count < c.Capacity).ToList();
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
                        Category = gymClass.Category.Name,
                        ClassType = gymClass.ClassType.Name,
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
                        Category = onlineClass.Category.Name,
                        ClassType = onlineClass.ClassType.Name,
                    });
                }
            }
            viewModel.Classes = viewModel.Classes.OrderBy(c => c.DateStart).ToList();
            return View(viewModel);
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> Register(int classId, bool isOnline)
        {
            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
            }

            var history = new RegisteredInClassesHistory
            {
                UserId = client.Id,
                ClassId = classId,
                RegistrationDate = DateTime.UtcNow,
                Canceled = false,
            };

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
                    await _registeredInClassesHistoryRepository.CreateAsync(history);
                }
            }
            else
            {
                var gymClass = await _classRepository.GetGymClassByIdIncludeTracked(classId);

                if(gymClass.Gym.Id != client.GymId)
                {
                    return ClassNotFound();
                }

                if (gymClass == null)
                {
                    return ClassNotFound();
                }

                if (gymClass.Clients.Count == gymClass.Capacity)
                {
                    return ClassNotFound();
                }

                if (!gymClass.Clients.Any(c => c.Id == client.Id))
                {
                    gymClass.Clients.Add(client);
                    await _classRepository.UpdateAsync(gymClass);
                    await _registeredInClassesHistoryRepository.CreateAsync(history);
                }
            }
            return RedirectToAction(nameof(AvailableClasses));
        }

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyClasses()
        {
            var client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
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
                        Category = gymClass.Category.Name,
                        ClassType = gymClass.ClassType.Name,
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
                        Category = onlineClass.Category.Name,
                        ClassType = onlineClass.ClassType.Name,
                    });
                }
            }
            viewModel.Classes = viewModel.Classes.OrderBy(c => c.DateStart).ToList();
            return View(viewModel);
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> Unregister(int id)
        {
            var client = await _userHelper.GetUserAsync(User) as Client;
            if (client == null)
            {
                return UserNotFound();
            }

            var historyEntry = await _registeredInClassesHistoryRepository.GetHistoryEntryAsync(id, client.Id);

            if (historyEntry != null)
            {
                historyEntry.Canceled = true;
                await _registeredInClassesHistoryRepository.UpdateAsync(historyEntry);
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

        [Authorize(Roles = "Client")]
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
                    GymName = gymClass.Gym?.Name,
                    ClassType = gymClass.ClassType.Name,

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
                    Platform = onlineClass.Platform,
                    ClassType = onlineClass.ClassType.Name,
                };
                return View(viewModel);
            }
            return ClassNotFound();
        }

       //Employee side actions

        [Authorize(Roles = "Employee")]
        public IActionResult FindClientByEmail()
        {
            return View();
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> FindClientByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
                var model = new RegisterClientInClassViewModel();
                return View("FindClientByEmail", model);
            }

            var client = await _userHelper.GetUserByEmailAsync(email) as Client;
            if (client == null)
            {
                ModelState.AddModelError(string.Empty, "Client not found.");
                var model = new RegisterClientInClassViewModel { ClientEmail = email };
                return View("FindClientByEmail", model);
            }
            return RedirectToAction(nameof(RegisterClientInClass), new { email });
        }

        public async Task<IActionResult> RegisterClientInClass(string email)
        {
            var client = await _userHelper.GetUserByEmailAsync(email) as Client;
            if (client == null)
            {
                return RedirectToAction("FindClientByEmail"); 
            }

            var employee = await _userHelper.GetUserAsync(this.User) as Employee;

            var classes = await _classRepository.GetAllGymClassesInclude();

            classes = classes.Where(c => c.Gym.Id == employee.GymId && c.Clients.Count < c.Capacity).ToList();

            var model = new RegisterClientInClassViewModel
            {
                ClientEmail = email,
                IsEmailValid = true,
                Classes = classes.Select(c => new ClassDetailsViewModel
                {
                    Id = c.Id,
                    Category = c.Category.Name,
                    ClassType = c.ClassType.Name,
                    InstructorName = c.Instructor.FullName,
                    DateStart = c.DateStart,
                    DateEnd = c.DateEnd,
                    Location = c.Gym.Name,
                    IsClientRegistered = c.Clients.Any(cl => cl.Id == client.Id)
                }).ToList()
            };

            return View("RegisterClientInClass", model);
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> RegisterClientInClassConfirm(RegisterClientInClassViewModel model)
        {
            var clientEmail = model.ClientEmail;

            var client = await _userHelper.GetUserByEmailAsync(model.ClientEmail) as Client;
            if (client == null)
            {
                ModelState.AddModelError(string.Empty, "Client not found.");
                model.Classes = await LoadClassDetails();
                return View("RegisterClientInClass", model);
            }

            var employee = await _userHelper.GetUserAsync(this.User);

            var allClasses = await _classRepository.GetAllGymClassesInclude();
            allClasses = allClasses.Where(c => c.Clients.Count < c.Capacity).ToList();

            foreach (var gymClass in allClasses)
            {
                var history = new RegisteredInClassesHistory
                {
                    UserId = client.Id,
                    ClassId = gymClass.Id,
                    EmployeeId = employee.Id,
                    RegistrationDate = DateTime.UtcNow,
                    Canceled = false,
                };
                if (model.SelectedClassIds.Contains(gymClass.Id))
                {
                    if (!gymClass.Clients.Any(c => c.Id == client.Id))
                    {
                        gymClass.Clients.Add(client);
                        await _registeredInClassesHistoryRepository.CreateAsync(history);
                    }
                }
                else
                {
                    if (gymClass.Clients.Any(c => c.Id == client.Id))
                    {
                        gymClass.Clients.Remove(client);

                        var historyEntry = await _registeredInClassesHistoryRepository.GetHistoryEntryAsync(gymClass.Id, client.Id);

                        if (historyEntry != null)
                        {
                            historyEntry.Canceled = true;
                            await _registeredInClassesHistoryRepository.UpdateAsync(historyEntry);
                        }
                    }
                }
                await _classRepository.UpdateAsync(gymClass);
            }
            return RedirectToAction("Clients", "Account");
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

        public IActionResult ClassNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Class not found", Message = "With so many available, how could you not find one?" });
        }

        public IActionResult UserNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "User not found", Message = "Looks like this user skipped leg day!" });
        }
    }
}


