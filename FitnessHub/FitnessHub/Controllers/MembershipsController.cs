using FitnessHub.Data.Entities.History;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Controllers
{
    public class MembershipsController : Controller
    {
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUserHelper _userHelper;
        private readonly IMembershipDetailsRepository _membershipDetailsRepository;
        private readonly IMembershipHistoryRepository _membershipHistoryRepository;
        private readonly IClientMembershipHistoryRepository _clientMembershipHistoryRepository;

        public MembershipsController(IMembershipRepository membershipRepository, IUserHelper userHelper, IMembershipDetailsRepository membershipDetailsRepository, IMembershipHistoryRepository membershipHistoryRepository, IClientMembershipHistoryRepository clientMembershipHistoryRepository)
        {
            _membershipRepository = membershipRepository;
            _userHelper = userHelper;
            _membershipDetailsRepository = membershipDetailsRepository;
            _membershipHistoryRepository = membershipHistoryRepository;
            _clientMembershipHistoryRepository = clientMembershipHistoryRepository;
        }

        [Authorize(Roles = "Client, Admin, MasterAdmin, Instructor, Employee")]
        [AllowAnonymous]
        public async Task<IActionResult> Available()
        {
            Client client = new();
            try
            {
                client = await _userHelper.GetUserAsync(this.User) as Client;
            }
            catch (Exception)
            {
            }

            if (client != null && await _userHelper.IsUserInRoleAsync(client, "Client") && client.MembershipDetailsId == null)
            {
                ViewBag.ShowSignUp = true;
            }

            return View(_membershipRepository.GetAll());
        }

        // GET: User Memberships History
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyMembershipHistory()
        {
            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
            }

            List<ClientMembershipHistory> memberships = await _clientMembershipHistoryRepository.GetAll().Where(m => m.UserId == client.Id).ToListAsync();
            List<ClientMembershipHistoryViewModel> models = new();

            foreach (var m in memberships)
            {
                var membership = await _membershipHistoryRepository.GetByIdAsync(m.MembershipHistoryId);

                models.Add(
                    new ClientMembershipHistoryViewModel
                    {
                        MembershipHistoryId = m.MembershipHistoryId,
                        DateRenewal = m.DateRenewal,
                        Id = m.Id,
                        Name = membership.Name,
                        Price = membership.Price,
                        Status = m.Status,
                        SignUpDate = m.SignUpDate,
                    }
                );
            }

            return View(models);
        }

        // GET: User Membership
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyMembership()
        {
            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
            }

            if (client.MembershipDetailsId == null)
            {
                ViewBag.HasMembership = false;
                return View(null);
            }

            MembershipDetails details = await _membershipDetailsRepository.GetMembershipDetailsByIdIncludeMembership(client.MembershipDetailsId.Value);

            return View(details);
        }

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> SignUp()
        {
            List<Membership> memberships = await _membershipRepository.GetAll().ToListAsync();

            List<SelectListItem> selectMembership = new();

            foreach (var m in memberships)
            {
                selectMembership.Add(new SelectListItem
                {
                    Text = m.Name,
                    Value = m.Id.ToString(),
                });
            }

            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
            }

            if (client.MembershipDetailsId == null)
            {
                ViewBag.HasMembership = false;
            }

            MembershipViewModel model = new();

            model.SelectMembership = selectMembership;

            return View(model);
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> SignUp(MembershipViewModel model)
        {
            if (model.MembershipId < 1)
            {
                ModelState.AddModelError("MembershipId", "Please select a valid Membership");
            }

            if (ModelState.IsValid)
            {
                MembershipDetails membershipDetails = new();

                var membership = await _membershipRepository.GetByIdTrackAsync(model.MembershipId);

                if (membership == null)
                {
                    return MembershipNotFound();
                }

                membershipDetails.Membership = membership;
                membershipDetails.DateRenewal = DateTime.UtcNow.AddMonths(12);
                membershipDetails.SignUpDate = DateTime.UtcNow;

                Client client = await _userHelper.GetUserAsync(this.User) as Client;

                if (client == null)
                {
                    return UserNotFound();
                }

                client.MembershipDetails = membershipDetails;
                client.IdentificationNumber = model.IdNumber;
                client.FullAddress = model.FullAddress;

                await _membershipDetailsRepository.CreateAsync(membershipDetails);
                await _userHelper.UpdateUserAsync(client);

                ClientMembershipHistory record = new()
                {
                    Id = membershipDetails.Id,
                    DateRenewal = membershipDetails.DateRenewal,
                    MembershipHistoryId = membershipDetails.Membership.Id,
                    UserId = client.Id,
                    SignUpDate = membershipDetails.SignUpDate,
                };

                await _clientMembershipHistoryRepository.CreateAsync(record);

                return RedirectToAction(nameof(MyMembership));
            }

            List<Membership> memberships = await _membershipRepository.GetAll().ToListAsync();

            List<SelectListItem> selectMembership = new();

            foreach (var m in memberships)
            {
                selectMembership.Add(new SelectListItem
                {
                    Text = m.Name,
                    Value = m.Id.ToString(),
                });
            }

            model.SelectMembership = selectMembership;

            return View(model);
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> CancelMembership()
        {
            Client client = await _userHelper.GetUserAsync(this.User) as Client;

            if (client == null)
            {
                return UserNotFound();
            }

            if (client.MembershipDetailsId == null)
            {
                return MembershipNotFound();
            }

            MembershipDetails details = await _membershipDetailsRepository.GetMembershipDetailsByIdIncludeMembership(client.MembershipDetailsId.Value);

            if (details == null)
            {
                return MembershipNotFound();
            }

            ClientMembershipHistory record = await _clientMembershipHistoryRepository.GetByIdAsync(details.Id);
            record.Status = false;

            await _clientMembershipHistoryRepository.UpdateAsync(record);

            await _membershipDetailsRepository.DeleteAsync(details);
            client.MembershipDetails = null;
            await _userHelper.UpdateUserAsync(client);

            return RedirectToAction(nameof(MyMembership));
        }

        // GET: MembershipsHistory
        [Authorize(Roles = "MasterAdmin")]
        public IActionResult MembershipsHistory()
        {
            return View(_membershipHistoryRepository.GetAll());
        }

        // GET: Memberships
        [Authorize(Roles = "MasterAdmin")]
        public IActionResult Index()
        {
            return View(_membershipRepository.GetAll());
        }

        // GET: Memberships/Details/5
        [Authorize(Roles = "MasterAdmin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return MembershipNotFound();
            }

            Membership membership = await _membershipRepository.GetByIdAsync(id.Value);

            if (membership == null)
            {
                return MembershipNotFound();
            }

            return View(membership);
        }

        // GET: Memberships/Create
        [Authorize(Roles = "MasterAdmin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Memberships/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "MasterAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Membership membership)
        {
            List<Membership> memberships = await _membershipRepository.GetAll().ToListAsync();

            foreach (var m in memberships)
            {
                if (m.Name == membership.Name)
                {
                    ModelState.AddModelError("Name", "There is already a Membership with this Name!");
                }
            }

            if (ModelState.IsValid)
            {
                await _membershipRepository.CreateAsync(membership);

                MembershipHistory record = new()
                {
                    Id = membership.Id,
                    Description = membership.Description,
                    Price = membership.Price,
                    Name = membership.Name,
                };

                await _membershipHistoryRepository.CreateAsync(record);

                return RedirectToAction(nameof(Index));
            }

            return View(membership);
        }

        // GET: Memberships/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return MembershipNotFound();
            }

            Membership membership = await _membershipRepository.GetByIdAsync(id.Value);

            if (membership == null)
            {
                return MembershipNotFound();
            }

            return View(membership);
        }

        // POST: Memberships/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "MasterAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Membership membership)
        {
            Membership original = await _membershipRepository.GetByIdAsync(membership.Id);

            if (original == null)
            {
                return MembershipNotFound();
            }

            List<Membership> memberships = await _membershipRepository.GetAll().ToListAsync();

            foreach (var m in memberships)
            {
                if (m.Name == membership.Name && m.Name != original.Name)
                {
                    ModelState.AddModelError("Name", "There is already a Membership with this Name!");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _membershipRepository.UpdateAsync(membership);

                    var record = await _membershipHistoryRepository.GetByIdAsync(membership.Id);
                    record.Description = membership.Description;
                    record.Price = membership.Price;
                    record.Name = membership.Name;

                    await _membershipHistoryRepository.UpdateAsync(record);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _membershipRepository.ExistsAsync(membership.Id))
                    {
                        return MembershipNotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(membership);
        }

        // GET: Memberships/Delete/5
        [Authorize(Roles = "MasterAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return MembershipNotFound();
            }

            Membership membership = await _membershipRepository.GetByIdAsync(id.Value);

            if (membership == null)
            {
                return MembershipNotFound();
            }

            return View(membership);
        }

        // POST: Memberships/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "MasterAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var membership = await _membershipRepository.GetByIdAsync(id);

            if (membership != null)
            {
                await _membershipRepository.DeleteAsync(membership);

                var record = await _membershipHistoryRepository.GetByIdAsync(id);
                record.Canceled = true;
                await _membershipHistoryRepository.UpdateAsync(record);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult MembershipNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Membership not found", Message = "Maybe its time to add another membership?" });
        }

        public IActionResult UserNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "User not found", Message = "Looks like this user skipped leg day!" });
        }
    }
}
