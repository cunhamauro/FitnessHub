using FitnessHub.Data;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using FitnessHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Controllers
{
    [Authorize(Roles = "MasterAdmin")]
    public class MembershipsController : Controller
    {
        private readonly IMembershipRepository _membershipRepository;

        public MembershipsController(IMembershipRepository membershipRepository)
        {
            _membershipRepository = membershipRepository;
        }

        [Authorize(Roles = "Client, Admin, MasterAdmin, Instructor, Employee")]
        [AllowAnonymous]
        public IActionResult Available()
        {
            return View(_membershipRepository.GetAll());
        }

        // GET: Memberships
        public IActionResult Index()
        {
            return View(_membershipRepository.GetAll());
        }

        // GET: Memberships/Details/5
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
        public async Task<IActionResult> Create()
        {
            List<SelectListItem> tierList = new List<SelectListItem>();
            List<Membership> memberships = await _membershipRepository.GetAll().ToListAsync();

            for (int i = 1; i <= 9; i++)
            {
                bool tierTaken = false;
                foreach (Membership membership in memberships)
                {
                    if (membership.Tier == i)
                    {
                        tierTaken = true;
                        break;
                    }
                }

                if (!tierTaken)
                {
                    tierList.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
                }
            }

            ViewBag.TierList = tierList;

            return View();
        }

        // POST: Memberships/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Membership membership)
        {
            List<Membership> memberships = await _membershipRepository.GetAll().ToListAsync();

            if (memberships.Any(m => m.Tier == membership.Tier))
            {
                ModelState.AddModelError("Tier", "This Tier is already assigned to another Membership");
            }

            if (ModelState.IsValid)
            {
                await _membershipRepository.CreateAsync(membership);

                return RedirectToAction(nameof(Index));
            }

            List<SelectListItem> tierList = new List<SelectListItem>();

            for (int i = 1; i <= 9; i++)
            {
                bool tierTaken = false;
                foreach (Membership m in memberships)
                {
                    if (m.Tier == i)
                    {
                        tierTaken = true;
                        break;
                    }
                }

                if (!tierTaken)
                {
                    tierList.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
                }
            }

            ViewBag.TierList = tierList;

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

            List<SelectListItem> tierList = new List<SelectListItem>();
            List<Membership> memberships = await _membershipRepository.GetAll().ToListAsync();

            for (int i = 1; i <= 9; i++)
            {
                bool tierTaken = false;
                foreach (Membership m in memberships)
                {
                    if (m.Tier == i)
                    {
                        tierTaken = true;
                        break;
                    }
                }

                if (!tierTaken)
                {
                    tierList.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
                }
            }

            tierList.Add(new SelectListItem { Value = membership.Tier.ToString(), Text = membership.Tier.ToString() });
            tierList = tierList.OrderBy(t => t.Value).ToList();

            ViewBag.TierList = tierList;

            return View(membership);
        }

        // POST: Memberships/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Membership membership)
        {
            if (membership == null)
            {
                return MembershipNotFound();
            }

            List<Membership> memberships = await _membershipRepository.GetAll().ToListAsync();

            if (memberships.Any(m => m.Tier == membership.Tier && m.Id != membership.Id))
            {
                ModelState.AddModelError("Tier", "This Tier is already assigned to another Membership");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _membershipRepository.UpdateAsync(membership);
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
            List<SelectListItem> tierList = new List<SelectListItem>();

            for (int i = 1; i <= 9; i++)
            {
                bool tierTaken = false;
                foreach (Membership m in memberships)
                {
                    if (m.Tier == i)
                    {
                        tierTaken = true;
                        break;
                    }
                }

                if (!tierTaken)
                {
                    tierList.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
                }
            }

            tierList.Add(new SelectListItem { Value = membership.Tier.ToString(), Text = membership.Tier.ToString() });
            tierList = tierList.OrderBy(t => t.Value).ToList();

            ViewBag.TierList = tierList;

            return View(membership);
        }

        // GET: Memberships/Delete/5
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var membership = await _membershipRepository.GetByIdAsync(id);

            if (membership != null)
            {
                await _membershipRepository.DeleteAsync(membership);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult MembershipNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Membership not found", Message = "Maybe its time to add another membership?" });
        }
    }
}
