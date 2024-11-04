using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.Communication;
using FitnessHub.Data.Entities.History;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessHub.Controllers
{
    public class CommunicationController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IGymRepository _gymRepository;
        private readonly IRequestInstructorRepository _requestInstructorRepository;
        private readonly IClientInstructorAppointmentRepository _clientInstructorAppointmentRepository;
        private readonly IRequestInstructorHistoryRepository _requestInstructorHistoryRepository;
        private readonly IClientInstructorAppointmentHistoryRepository _clientInstructorAppointmentHistoryRepository;

        public CommunicationController(
            IUserHelper userHelper,
            IGymRepository gymRepository,
            IRequestInstructorRepository requestInstructorRepository,
            IClientInstructorAppointmentRepository clientInstructorAppointmentRepository,
            IRequestInstructorHistoryRepository requestInstructorHistoryRepository,
            IClientInstructorAppointmentHistoryRepository clientInstructorAppointmentHistoryRepository)
        {
            _userHelper = userHelper;
            _gymRepository = gymRepository;
            _requestInstructorRepository = requestInstructorRepository;
            _clientInstructorAppointmentRepository = clientInstructorAppointmentRepository;
            _requestInstructorHistoryRepository = requestInstructorHistoryRepository;
            _clientInstructorAppointmentHistoryRepository = clientInstructorAppointmentHistoryRepository;
        }

        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> RequestInstructor()
        {
            var client = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
            if (client == null)
            {
                return ClientNotFound();
            }

            var model = new RequestInstructorViewModel
            {
                Gyms = _gymRepository.GetAll().Select(gym => new SelectListItem
                {
                    Value = gym.Id.ToString(),
                    Text = $"{gym.Data}",
                })
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RequestInstructor(RequestInstructorViewModel model)
        {
            if (model.GymId < 1)
            {
                ModelState.AddModelError("Gym", "Please select a gym.");
            }

            if (ModelState.IsValid)
            {
                var client = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                if (client == null)
                {
                    return ClientNotFound();
                }

                var gym = await _gymRepository.GetByIdTrackAsync(model.GymId);
                if (gym == null)
                {
                    return GymNotFound();
                }

                if(await _requestInstructorRepository.ClientHasPendingRequestForGym(client.Id, gym.Id))
                {
                    ModelState.AddModelError("GymId", "There is already a pending request for the selected gym.");

                    model.Gyms = _gymRepository.GetAll().Select(gym => new SelectListItem
                    {
                        Value = gym.Id.ToString(),
                        Text = $"{gym.Data}",
                    });

                    return View(model);
                }

                try
                {
                    var requestInstructor = new RequestInstructor
                    {
                        Client = client as Client,
                        Gym = gym,
                        Notes = model.Notes,
                    };

                    await _requestInstructorRepository.CreateAsync(requestInstructor);

                    var requestHistory = new RequestInstructorHistory
                    {
                        Id = requestInstructor.Id,
                        ClientId = client.Id,
                        GymId = gym.Id,
                        Notes = model.Notes,
                        RequestDate = requestInstructor.RequestDate,
                        IsResolved = false,
                    };

                    await _requestInstructorHistoryRepository.CreateAsync(requestHistory);

                    return RedirectToAction(nameof(MyRequests));
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                }
            }

            model.Gyms = _gymRepository.GetAll().Select(gym => new SelectListItem
            {
                Value = gym.Id.ToString(),
                Text = $"{gym.Data}",
            });

            return View(model);
        }

        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> MyRequests()
        {
            var client = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name) as Client;
            if (client == null)
            {
                return ClientNotFound();
            }

            var requests = _requestInstructorHistoryRepository.GetAllByClient(client.Id);

            List<MyRequestInstructorHistoryViewModel> requestsModel = new List<MyRequestInstructorHistoryViewModel>();

            foreach (var request in requests)
            {
                var gym = await _gymRepository.GetByIdAsync(request.GymId);
                if (gym == null)
                {
                    return GymNotFound();
                }

                string status = string.Empty;

                if (request.IsResolved)
                    status = "Resolved";
                else
                    status = "Pending";

                requestsModel.Add(new MyRequestInstructorHistoryViewModel()
                {
                    Gym = gym.Name,
                    Notes = request.Notes,
                    RequestDate = request.RequestDate,
                    Status = status,
                });
            }

            return View(requestsModel);
        }

        [Authorize(Roles = "Employee")]
        [HttpGet]
        public async Task<IActionResult> ClientsRequests()
        {
            var employee = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name) as Employee;
            if (employee == null)
            {
                return EmployeeNotFound();
            }

            var gym = await _gymRepository.GetByIdAsync(employee.GymId.Value);
            if (gym == null)
            {
                return GymNotFound();
            }

            var requests = _requestInstructorRepository.GetAllByGymWithClients(gym);

            return View(requests);
        }

        [Authorize(Roles = "Employee, Admin")]
        [HttpGet]
        public async Task<IActionResult> ClientsRequestsHistory()
        {
            var gym = new Gym();

            if (this.User.IsInRole("Employee"))
            {
                var employee = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name) as Employee;
                if (employee == null)
                {
                    return EmployeeNotFound();
                }

                gym = await _gymRepository.GetByIdAsync(employee.GymId.Value);
                if (gym == null)
                {
                    return GymNotFound();
                }
            }

            if (this.User.IsInRole("Admin"))
            {
                var admin = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name) as Admin;
                if (admin == null)
                {
                    return AdminNotFound();
                }

                gym = await _gymRepository.GetByIdAsync(admin.GymId.Value);
                if (gym == null)
                {
                    return GymNotFound();
                }
            }

            var requests = _requestInstructorHistoryRepository.GetAllByGymId(gym.Id);

            List<RequestInstructorHistoryViewModel> requestsModel = new List<RequestInstructorHistoryViewModel>();

            foreach (var request in requests)
            {
                var client = await _userHelper.GetUserByIdAsync(request.ClientId);
                if (client == null)
                {
                    return ClientNotFound();
                }

                requestsModel.Add(new RequestInstructorHistoryViewModel()
                {
                    Id = request.Id,
                    ClientName = client.FullName,
                    ClientEmail = client.Email,
                    Notes = request.Notes,
                    RequestDate = request.RequestDate,
                });
            }

            return View(requestsModel);
        }

        [Authorize(Roles = "Employee")]
        [HttpGet]
        public async Task<IActionResult> AssignInstructor(int id)
        {
            var employee = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name) as Employee;
            if (employee == null)
            {
                return EmployeeNotFound();
            }

            var gym = await _gymRepository.GetByIdAsync(employee.GymId.Value);
            if (gym == null)
            {
                return GymNotFound();
            }

            var request = await _requestInstructorRepository.GetByIdWithClientAndGym(id);
            if (request == null)
            {
                return RequestNotFound();
            }

            var client = await _userHelper.GetUserByIdAsync(request.Client.Id);
            if (client == null)
            {
                return ClientNotFound();
            }

            var instructors = await _userHelper.GetInstructorsByGymAsync(gym.Id);

            var model = new AssignInstructorViewModel
            {
                RequestId = request.Id,
                ClientId = request.Client.Id,
                EmployeeId = employee.Id,
                GymId = gym.Id,
                Instructors = instructors.Select(instructor => new SelectListItem
                {
                    Value = instructor.Id,
                    Text = $"{instructor.FullName}",
                })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignInstructor(AssignInstructorViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.InstructorId))
            {
                ModelState.AddModelError("InstructorId", "Please select an instructor.");
            }

            var client = await _userHelper.GetUserByIdAsync(model.ClientId);
            if (client == null)
            {
                return ClientNotFound();
            }

            if (ModelState.IsValid)
            {
                var instructor = await _userHelper.GetUserByIdAsync(model.InstructorId);
                if (instructor == null)
                {
                    return InstructorNotFound();
                }

                var request = await _requestInstructorRepository.GetByIdWithClientAndGym(model.RequestId);
                if (request == null)
                {
                    return RequestNotFound();
                }

                var requestHistory = await _requestInstructorHistoryRepository.GetByIdTrackAsync(model.RequestId);
                if (requestHistory == null)
                {
                    return RequestNotFound();
                }

                try
                {
                    var appointment = new ClientInstructorAppointment
                    {
                        Client = client as Client,
                        EmployeeId = model.EmployeeId,
                        InstructorId = instructor.Id
                    };

                    await _clientInstructorAppointmentRepository.CreateAsync(appointment);

                    var appointmentHistory = new ClientInstructorAppointmentHistory
                    {
                        Id = appointment.Id,
                        ClientId = client.Id,
                        EmployeeId = model.EmployeeId,
                        InstructorId = instructor.Id,
                        GymId = model.GymId,
                        AssignDate = appointment.AssignDate,
                    };

                    await _clientInstructorAppointmentHistoryRepository.CreateAsync(appointmentHistory);

                    requestHistory.IsResolved = true;
                    await _requestInstructorHistoryRepository.UpdateAsync(requestHistory);

                    await _requestInstructorRepository.DeleteAsync(request);

                    return RedirectToAction(nameof(ClientsRequests));
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                }
            }

            var gym = await _gymRepository.GetGymByUserAsync(client);
            if (gym == null)
            {
                return GymNotFound();
            }

            var instructors = await _userHelper.GetInstructorsByGymAsync(gym.Id);

            model.Instructors = instructors.Select(instructor => new SelectListItem
            {
                Value = instructor.Id,
                Text = $"{instructor.FullName}",
            });

            return View(model);
        }

        [Authorize(Roles = "Employee, Admin")]
        [HttpGet]
        public async Task<IActionResult> AssignInstructorHistory(int id)
        {
            var gym = new Gym();

            if (this.User.IsInRole("Employee"))
            {
                var employee = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name) as Employee;
                if (employee == null)
                {
                    return EmployeeNotFound();
                }

                gym = await _gymRepository.GetByIdAsync(employee.GymId.Value);
                if (gym == null)
                {
                    return GymNotFound();
                }
            }

            if (this.User.IsInRole("Admin"))
            {
                var admin = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name) as Admin;
                if (admin == null)
                {
                    return AdminNotFound();
                }

                gym = await _gymRepository.GetByIdAsync(admin.GymId.Value);
                if (gym == null)
                {
                    return GymNotFound();
                }
            }

            var assignments = _clientInstructorAppointmentHistoryRepository.GetAllByGymId(gym.Id);

            List<ClientInstructorAppointmentHistoryViewModel> assignmentsModel = new List<ClientInstructorAppointmentHistoryViewModel>();

            foreach (var assignment in assignments)
            {
                var client = await _userHelper.GetUserByIdAsync(assignment.ClientId);
                if (client == null)
                {
                    return ClientNotFound();
                }

                var instructor = await _userHelper.GetUserByIdAsync(assignment.InstructorId);
                if (client == null)
                {
                    return InstructorNotFound();
                }

                var employee = await _userHelper.GetUserByIdAsync(assignment.EmployeeId);
                if (employee == null)
                {
                    return EmployeeNotFound();
                }

                assignmentsModel.Add(new ClientInstructorAppointmentHistoryViewModel()
                {
                    Id = assignment.Id,
                    ClientName = client.FullName,
                    ClientEmail = client.Email,
                    InstructorEmail = instructor.Email,
                    InstructorName = instructor.FullName,
                    EmployeeEmail = employee.Email,
                    EmployeeName = employee.FullName,
                    AssignDate = assignment.AssignDate,
                });
            }

            return View(assignmentsModel);
        }

        [Authorize(Roles = "Instructor")]
        [HttpGet]
        public async Task<IActionResult> ClientsAssignments()
        {
            var instructor = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name) as Instructor;
            if (instructor == null)
            {
                return InstructorNotFound();
            }

            var gym = await _gymRepository.GetByIdAsync(instructor.GymId.Value);
            if (gym == null)
            {
                return GymNotFound();
            }

            var assignments = _clientInstructorAppointmentRepository.GetAllByInstructorWithClients(instructor.Id);

            return View(assignments);
        }

        public IActionResult ClientNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Client not found", Message = "Looks like this client skipped leg day!" });
        }

        public IActionResult InstructorNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Instructor not found", Message = "Looks like this instructor skipped leg day!" });
        }

        public IActionResult EmployeeNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Employee not found", Message = "Looks like this employee skipped leg day!" });
        }

        public IActionResult AdminNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Admin not found", Message = "Looks like this admin skipped leg day!" });
        }

        public IActionResult GymNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Gym not found", Message = "With so many worldwide, how did you miss this one?" });
        }

        public IActionResult RequestNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Request not found", Message = "No requests match that ID..." });
        }
    }
}
