using FitnessHub.Data;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Controllers
{
    public class MembershipsController : Controller
    {
        private readonly IMembershipRepository _membershipRepository;

        public MembershipsController(IMembershipRepository membershipRepository)
        {
            _membershipRepository = membershipRepository;
        }

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
                return NotFound();
            }

            Membership membership = await _membershipRepository.GetByIdAsync(id.Value);

            if (membership == null)
            {
                return NotFound();
            }

            return View(membership);
        }

        // GET: Memberships/Create
        public async Task<IActionResult> Create()
        {
            List<SelectListItem> tierList = new List<SelectListItem>();
            List<Membership> memberships = await _membershipRepository.GetAll().ToListAsync();

            for (int i = 0; i <= 9; i++)
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
            List<Membership> memberhsips = await _membershipRepository.GetAll().ToListAsync();

            if (memberhsips.Any(m => m.Tier == membership.Tier))
            {
                ModelState.AddModelError("Tier", "This Tier is already selected");
            }

            if (ModelState.IsValid)
            {
                await _membershipRepository.CreateAsync(membership);

                return RedirectToAction(nameof(Index));
            }
            return View(membership);
        }

        // GET: Memberships/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Membership membership = await _membershipRepository.GetByIdAsync(id.Value);

            if (membership == null)
            {
                return NotFound();
            }

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
                return NotFound();
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
                        return NotFound();
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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Membership membership = await _membershipRepository.GetByIdAsync(id.Value);

            if (membership == null)
            {
                return NotFound();
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
    }
}
