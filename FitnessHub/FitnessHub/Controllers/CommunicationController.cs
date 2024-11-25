using FitnessHub.Data.Classes;
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
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        private readonly IMailHelper _mailHelper;
        private readonly IForumRepository _forumRepository;
        private readonly IImageHelper _imageHelper;
        private readonly IForumReplyRepository _forumReplyRepository;
        private readonly ITicketRepository _ticketRepository;

        public CommunicationController(
            IUserHelper userHelper,
            IGymRepository gymRepository,
            IRequestInstructorRepository requestInstructorRepository,
            IClientInstructorAppointmentRepository clientInstructorAppointmentRepository,
            IRequestInstructorHistoryRepository requestInstructorHistoryRepository,
            IClientInstructorAppointmentHistoryRepository clientInstructorAppointmentHistoryRepository,
            IMailHelper mailHelper, IForumRepository forumRepository, IImageHelper imageHelper, IForumReplyRepository forumReplyRepository, ITicketRepository ticketRepository)
        {
            _userHelper = userHelper;
            _gymRepository = gymRepository;
            _requestInstructorRepository = requestInstructorRepository;
            _clientInstructorAppointmentRepository = clientInstructorAppointmentRepository;
            _requestInstructorHistoryRepository = requestInstructorHistoryRepository;
            _clientInstructorAppointmentHistoryRepository = clientInstructorAppointmentHistoryRepository;
            _mailHelper = mailHelper;
            _forumRepository = forumRepository;
            _imageHelper = imageHelper;
            _forumReplyRepository = forumReplyRepository;
            _ticketRepository = ticketRepository;
        }

        // TICKET ACTIONS

        [Authorize(Roles = "Employee, Admin")]
        [HttpGet]
        public async Task<IActionResult> ClientTickets()
        {
            User user = await _userHelper.GetUserAsync(this.User) as User;

            if (user == null)
            {
                return ClientNotFound();
            }

            // Get all opened tickets with related data
            var ticketsList = await _ticketRepository
                .GetAll()
                .Include(t => t.TicketMessages).ThenInclude(t => t.User)
                .Include(t => t.Client)
                .Where(t => t.Open == true && (t.Picked == false || t.Staff == user))
                .ToListAsync();

            return View(ticketsList);
        }

        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> Tickets()
        {
            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return ClientNotFound();
            }

            var ticketsList = await _ticketRepository.GetTicketsByUserTrackIncludeAsync(client.Id);

            var tickets = await _ticketRepository.GetAll().Where(t => t.Client.Id == client.Id && t.Open == true).ToListAsync();
            
            if (tickets.Any())
            {
                ViewBag.AllowNew = false;
            }

            return View(ticketsList);
        }

        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> CreateTicket()
        {
            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return ClientNotFound();
            }

            var tickets = await _ticketRepository.GetAll().Where(t => t.Client.Id == client.Id && t.Open == true).ToListAsync();

            if (tickets.Any())
            {
                return RedirectToAction("Tickets");
            }

            return View();
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> CreateTicket(Ticket ticket)
        {
            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return ClientNotFound();
            }

            var tickets = await _ticketRepository.GetAll().Where(t => t.Client.Id == client.Id && t.Open == true).ToListAsync();

            if (tickets.Any())
            {
                return RedirectToAction("Tickets");
            }

            ticket.Client = client;
            ticket.DateOpened = DateTime.UtcNow;

            await _ticketRepository.CreateAsync(ticket);

            return RedirectToAction(nameof(Tickets));
        }

        [Authorize(Roles = "Client, Employee, Admin")]
        [HttpGet]
        public async Task<IActionResult> OpenTicket(int id)
        {
            if (id < 1)
            {
                return TicketNotFound();
            }

            User user = await _userHelper.GetUserAsync(this.User);

            if (user == null)
            {
                return ClientNotFound();
            }

            var ticket = await _ticketRepository.GetTicketByIdTrackIncludeAsync(id);

            if (ticket == null)
            {
                return TicketNotFound();
            }

            if (ticket.Staff != null && ticket.Staff != user && ticket.Client != user)
            {
                return TicketNotFound();
            }

            if (await _userHelper.IsUserInRoleAsync(user, "Employee") || await _userHelper.IsUserInRoleAsync(user, "Admin"))
            {
                ViewBag.AdminOrEmployee = true;
            }

            if (ticket.Open == false)
            {
                ViewBag.TicketClosed = true;
            }

            return View(ticket);
        }

        [Authorize(Roles = "Client, Employee, Admin")]
        [HttpPost]
        public async Task<IActionResult> ReplyTicket(int id, string Message)
        {
            User user = await _userHelper.GetUserAsync(this.User);

            if (user == null)
            {
                return ClientNotFound();
            }

            Ticket ticket = await _ticketRepository.GetTicketByIdTrackIncludeAsync(id);

            if (ticket == null)
            {
                return TicketNotFound();
            }

            // TODO: Check if is client assigned and if it is assigned, its the corresponding
            if (ticket.Client == user || ticket.Staff == null || ticket.Staff != null && user == ticket.Staff)
            {
                if (string.IsNullOrWhiteSpace(Message))
                {
                    ViewBag.Error = "Please write a valid message";
                    return View(ticket);
                }

                if (ticket.Open == false)
                {
                    ViewBag.Error = "This ticket is no longer open";
                    return View(ticket);
                }

                if (ticket.Picked == false && (await _userHelper.IsUserInRoleAsync(user, "Employee") || await _userHelper.IsUserInRoleAsync(user, "Admin")))
                {
                    ticket.DatePicked = DateTime.UtcNow;
                    ticket.Staff = user;
                    ticket.Picked = true;
                }

                if(this.User.IsInRole("Admin") || this.User.IsInRole("Employee"))
                {
                    ticket.HasStaffReply = true;
                    ticket.HasClientReply = false;
                }

                if (this.User.IsInRole("Client"))
                {
                    ticket.HasStaffReply = false;
                    ticket.HasClientReply = true;
                }

                var role = User.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .FirstOrDefault();

                var ticketMessage = new TicketMessage()
                {
                    DateTime = DateTime.UtcNow,
                    Message = Message,
                    User = user,
                    UserRole = role,
                };

                ticket.TicketMessages.Add(ticketMessage);

                await _ticketRepository.UpdateAsync(ticket);

                string classesUrl = Url.Action("AvailableClasses", "ClientClasses");
                string body = _mailHelper.GetEmailTemplate($"Ticket Answered", @$"Hey, {ticket.Client.FirstName}, the Ticket ({ticket.Title}) you opened at {ticket.DateOpened} as been answered by our {role} <span style=""font-weight: bold"">{user.FullName}</span>.", "Check our available classes");
                Response response = await _mailHelper.SendEmailAsync(ticket.Client.Email, "Ticket answered", body, null, null);
            }
            return RedirectToAction("OpenTicket", new { id = id });
        }

        [Authorize(Roles = "Employee, Admin")]
        [HttpPost]
        public async Task<IActionResult> CloseTicket(int id)
        {
            if (id < 1)
            {
                return TicketNotFound();
            }

            User user = await _userHelper.GetUserAsync(this.User);

            if (user == null)
            {
                return UserNotFound();
            }

            var ticket = await _ticketRepository.GetTicketByIdTrackIncludeAsync(id);

            if (ticket == null)
            {
                return TicketNotFound();
            }

            if (!await _userHelper.IsUserInRoleAsync(user, "Employee") && !await _userHelper.IsUserInRoleAsync(user, "Admin"))
            {
                return RedirectToAction(nameof(Tickets));
            }

            if (ticket.Staff == null)
            {
                ticket.Staff = user;
                ticket.Picked = true;
                ticket.DatePicked = DateTime.UtcNow;
            }

            ticket.Open = false;

            await _ticketRepository.UpdateAsync(ticket);

            string classesUrl = Url.Action("AvailableClasses", "ClientClasses");
            string body = _mailHelper.GetEmailTemplate($"Ticket Closed", @$"Hey, {ticket.Client.FirstName}, the Ticket ({ticket.Title}) you opened at {ticket.DateOpened} as been closed at {DateTime.UtcNow.ToString("dd-MM-yyyy HH:mm")}", "Check our available classes");
            Response response = await _mailHelper.SendEmailAsync(ticket.Client.Email, "Ticket closed", body, null, null);

            return RedirectToAction(nameof(ClientTickets));
        }


        // FORUM ACTIONS

        [Route("Forum/CreateThread")]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CreatePost()
        {
            var posts = await _forumRepository.GetAll().Include(f => f.User).Include(f => f.ForumReplies).ToListAsync();

            List<string> existingThemes = posts.Select(p => p.Theme).Distinct().ToList();

            ViewBag.Themes = existingThemes;

            return View();
        }

        [Route("Forum/CreateThread")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost(ForumThread newPost)
        {
            User user = await _userHelper.GetUserAsync(this.User);

            var role = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .FirstOrDefault();

            newPost.User = user;
            newPost.UserRole = role;
            newPost.Date = DateTime.UtcNow;

            var posts = await _forumRepository.GetAll().Include(f => f.User).Include(f => f.ForumReplies).ToListAsync();
            List<string> existingThemes = posts.Select(p => p.Theme).Distinct().ToList();
            ViewBag.Themes = existingThemes;

            if (newPost.ImageFile != null && newPost.ImageFile.Length > 0)
            {
                var path = await _imageHelper.UploadImageAsync(newPost.ImageFile, "forum");
                newPost.ImagePath = path;
            }

            await _forumRepository.CreateAsync(newPost);

            return RedirectToAction("Posts");
        }

        [Route("Forum")]
        [Authorize]
        public async Task<IActionResult> Posts()
        {
            User user = await _userHelper.GetUserAsync(this.User);

            if (user == null)
            {
                return UserNotFound();
            }

            var posts = await _forumRepository.GetAll().Include(f => f.User).Include(f => f.ForumReplies).ToListAsync();

            if (await _userHelper.IsUserInRoleAsync(user, "MasterAdmin"))
            {
                foreach (var post in posts)
                {
                    post.Owner = true;
                }
            }
            else
            {
                foreach (var post in posts)
                {
                    if (post.User.Id == user.Id || await _userHelper.IsUserInRoleAsync(user, "Admin"))
                    {
                        post.Owner = true;
                    }
                }

            }

            return View(posts);
        }

        [Route("Forum/Thread/{id}")]
        [Authorize]
        public async Task<IActionResult> OpenPost(int id)
        {
            if (id < 1)
            {
                return ThreadNotFound();
            }

            var post = await _forumRepository.GetForumPostByIdTrackIncludeAsync(id);

            if (post == null)
            {
                return ThreadNotFound();
            }

            return View(post);
        }

        [Route("Forum/ReplyThread/{id}")]
        [HttpGet]
        public async Task<IActionResult> ReplyPost(int id)
        {
            if (id < 1)
            {
                return ThreadNotFound();
            }

            var post = await _forumRepository.GetForumPostByIdTrackIncludeAsync(id);

            if (post == null)
            {
                return ThreadNotFound();
            }

            var newPostMessage = new ForumReply()
            {
                NavigatePost = id,
            };

            return View(newPostMessage);
        }

        [Route("Forum/ReplyThread")]
        [HttpPost]
        public async Task<IActionResult> ReplyPost(ForumReply reply)
        {
            User user = await _userHelper.GetUserAsync(this.User);

            if (user == null)
            {
                return UserNotFound();
            }

            reply.Id = 0;

            var role = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .FirstOrDefault();

            reply.User = user;
            reply.UserRole = role;
            reply.Date = DateTime.UtcNow;

            if (reply.NavigatePost < 1)
            {
                return ThreadNotFound();
            }

            var post = await _forumRepository.GetForumPostByIdTrackIncludeAsync(reply.NavigatePost);

            if (post == null)
            {
                return ThreadNotFound();
            }

            if (reply.ImageFile != null && reply.ImageFile.Length > 0)
            {
                var path = await _imageHelper.UploadImageAsync(reply.ImageFile, "forum");
                reply.ImagePath = path;
            }

            post.ForumReplies.Add(reply);

            await _forumRepository.UpdateAsync(post);

            return RedirectToAction("OpenPost", new { id = reply.NavigatePost });
        }

        [Route("Forum/DeleteThread/{id}")]
        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            User user = await _userHelper.GetUserAsync(this.User);

            if (user == null)
            {
                return UserNotFound();
            }

            if (id < 1)
            {
                return ThreadNotFound();
            }

            var post = await _forumRepository.GetForumPostByIdTrackIncludeAsync(id);

            if (post == null)
            {
                return ThreadNotFound();
            }

            // Auth

            bool valid = false;

            if (await _userHelper.IsUserInRoleAsync(user, "MasterAdmin"))
            {
                valid = true;
            }
            else if (await _userHelper.IsUserInRoleAsync(user, "Admin") && !await _userHelper.IsUserInRoleAsync(post.User, "MasterAdmin"))
            {
                valid = true;
            }
            else if (post.User.Id == user.Id)
            {
                valid = true;
            }

            if (!valid)
            {
                return ThreadNotFound();
            }

            List<ForumReply> replies = new List<ForumReply>(post.ForumReplies);

            foreach (var r in replies)
            {
                await _forumReplyRepository.DeleteAsync(r);
            }

            await _forumRepository.DeleteAsync(post);

            return RedirectToAction("Posts");
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

                if (await _requestInstructorRepository.ClientHasPendingRequestForGym(client.Id, gym.Id))
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
                        GymId = gym.Id,
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

                    return RedirectToAction("ClientHistory","Account");
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

            var requests = _requestInstructorRepository.GetAllByGymWithClients(gym.Id);

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

                var status = string.Empty;
                if (request.IsResolved)
                    status = "Resolve";
                else
                    status = "Pending";

                requestsModel.Add(new RequestInstructorHistoryViewModel()
                {
                    Id = request.Id,
                    ClientName = client.FullName,
                    ClientEmail = client.Email,
                    Notes = request.Notes,
                    RequestDate = request.RequestDate,
                    Status = status,
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

            var request = await _requestInstructorRepository.GetRequestByIdWithClient(id);

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

                var request = await _requestInstructorRepository.GetRequestByIdWithClient(model.RequestId);
                if (request == null)
                {
                    return RequestNotFound();
                }

                var requestHistory = await _requestInstructorHistoryRepository.GetByIdTrackAsync(model.RequestId);
                if (requestHistory == null)
                {
                    return RequestNotFound();
                }

                requestHistory.IsResolved = true;

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

                    await _requestInstructorHistoryRepository.UpdateAsync(requestHistory);

                    await _requestInstructorRepository.DeleteAsync(request);

                    string assignmentsUrl = Url.Action("ClientsAssignments", "Communication");

                    string body = _mailHelper.GetEmailTemplate($"Client Request Assignment", @$"Hey, {instructor.FirstName}, you were assigned to the client <span style=""font-weight: bold"">{client.FullName} [{client.Email}]</span>.", "Check your other client assignments");

                    Response response = await _mailHelper.SendEmailAsync(instructor.Email, "Client assigned", body, null, null);

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

                var status = string.Empty;

                if (assignment.IsResolved)
                    status = "Resolved";
                else
                    status = "Pending";

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
                    Status = status,
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

        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public async Task<IActionResult> ResolveAssignment(int id)
        {
            var instructor = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name) as Instructor;
            if (instructor == null)
            {
                return InstructorNotFound();
            }

            var assignment = await _clientInstructorAppointmentRepository.GetByIdAsync(id);
            if (assignment == null)
            {
                return AssignmentNotFound();
            }

            var assignmentHistory = await _clientInstructorAppointmentHistoryRepository.GetByIdTrackAsync(assignment.Id);
            if (assignmentHistory == null)
            {
                return AssignmentNotFound();
            }

            assignmentHistory.IsResolved = true;

            try
            {
                await _clientInstructorAppointmentHistoryRepository.UpdateAsync(assignmentHistory);

                await _clientInstructorAppointmentRepository.DeleteAsync(assignment);

                return RedirectToAction(nameof(ClientsAssignments));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IActionResult ClientNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Client not found", Message = "Looks like this client skipped leg day!" });
        }

        public IActionResult UserNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "User not found", Message = "Looks like this user skipped leg day!" });
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

        public IActionResult AssignmentNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Assignment not found", Message = "No assignments match that ID..." });
        }

        public IActionResult ThreadNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Thread not found", Message = "Hey, maybe it's time for you to start one!" });
        }

        public IActionResult TicketNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Ticket not found", Message = "Ticket got lost? Just open another one!" });
        }
    }
}