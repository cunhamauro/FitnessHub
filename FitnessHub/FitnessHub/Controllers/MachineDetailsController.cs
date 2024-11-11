using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Repositories;
using FitnessHub.Helpers;
using FitnessHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessHub.Controllers
{
    public class MachineDetailsController : Controller
    {
        private readonly IMachineDetailsRepository _machineDetailsRepository;
        private readonly IGymRepository _gymRepository;
        private readonly IMachineRepository _machineRepository;
        private readonly IUserHelper _userHelper;

        public MachineDetailsController(
            IMachineDetailsRepository machineDetailsRepository,
            IGymRepository gymRepository,
            IMachineRepository machineRepository,
            IUserHelper userHelper)
        {
            _machineDetailsRepository = machineDetailsRepository;
            _gymRepository = gymRepository;
            _machineRepository = machineRepository;
            _userHelper = userHelper;
        }

        // GET: MachineDetails
        [Authorize(Roles = "Employee, Admin")]
        public async Task<IActionResult> Index()
        {
            var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
            if (user == null)
            {
                return UserNotFound();
            }

            var gym = await _gymRepository.GetGymByUserAsync(user);
            if (gym == null)
            {
                return GymNotFound();
            }

            return View(_machineDetailsRepository.GetAllByGymWithMachinesAndGyms(gym.Id));
        }

        // GET: MachineDetails/Details/5
        [Authorize(Roles = "Employee, Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return MachineDetailsNotFound();
            }

            var machineDetails = await _machineDetailsRepository.GetByIdWithMachines(id.Value);
            if (machineDetails == null)
            {
                return MachineDetailsNotFound();
            }

            return View(machineDetails);
        }

        // GET: MachineDetails/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new MachineDetailsViewModel
            {
                Machines = await _machineRepository.GetAllMachinesSelectListWithoutNoMachine()
            };

            return View(model);
        }

        // POST: MachineDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MachineDetailsViewModel model)
        {
            if (model.MachineId < 1)
            {
                ModelState.AddModelError("MachineId", "Please select a valid Machine");
            }

            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                if (user == null)
                {
                    return UserNotFound();
                }

                var gym = await _gymRepository.GetGymByUserAsync(user);
                if (gym == null)
                {
                    return GymNotFound();
                }

                var maxMachineNumber = await _machineDetailsRepository.GetAllByGymWithMachinesAndGyms(gym.Id).MaxAsync(md => (int?)md.MachineNumber) ?? 0;

                var machineDetails = new MachineDetails
                {
                    Machine = await _machineRepository.GetByIdTrackAsync(model.MachineId),
                    Gym = gym,
                    Status = true,
                    MachineNumber = maxMachineNumber + 1
                };

                await _machineDetailsRepository.CreateAsync(machineDetails);

                return RedirectToAction(nameof(Index));
            }

            model.Machines = await _machineRepository.GetAllMachinesSelectListWithoutNoMachine();

            return View(model);
        }

        // POST: MachineDetails/ChangeStatus/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employee, Admin")]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            try
            {
                var machineDetails = await _machineDetailsRepository.GetByIdTrackAsync(id);
                if (machineDetails == null)
                {
                    return MachineDetailsNotFound();
                }

                machineDetails.Status = !machineDetails.Status;

                await _machineDetailsRepository.UpdateAsync(machineDetails);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _machineDetailsRepository.ExistsAsync(id))
                {
                    return MachineDetailsNotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: MachineDetails/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return MachineDetailsNotFound();
            }

            var machineDetails = await _machineDetailsRepository.GetByIdWithMachines(id.Value);
            if (machineDetails == null)
            {
                return MachineDetailsNotFound();
            }

            return View(machineDetails);
        }

        // POST: MachineDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var machineDetails = await _machineDetailsRepository.GetByIdAsync(id);
            if (machineDetails != null)
            {
                await _machineDetailsRepository.DeleteAsync(machineDetails);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult UserNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "User not found", Message = "Looks like this user skipped leg day!" });
        }

        public IActionResult GymNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Gym not found", Message = "With so many worldwide, how did you miss this one?" });
        }

        public IActionResult MachineDetailsNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Machine not found", Message = "Don't worry, there are plenty of other machines to get fit!" });
        }
    }
}