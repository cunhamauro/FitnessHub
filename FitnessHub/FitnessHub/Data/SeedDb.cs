using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.GymClasses;
using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Entities.History;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Helpers;

namespace FitnessHub.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IUserHelper _userHelper;

        public SeedDb(
            DataContext context,
            IConfiguration configuration,
            IUserHelper userHelper)
        {
            _context = context;
            _configuration = configuration;
            _userHelper = userHelper;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();

            await _userHelper.CheckRoleAsync("MasterAdmin");
            await _userHelper.CheckRoleAsync("Admin");
            await _userHelper.CheckRoleAsync("Employee");
            await _userHelper.CheckRoleAsync("Instructor");
            await _userHelper.CheckRoleAsync("Client");

            var noMachine = new Machine
            {
                Name = "No Machine",
                Category = null,
            };

            if (!_context.Machines.Any())
            {
                await _context.Machines.AddAsync(noMachine);
            }

            var user = await _userHelper.GetUserByEmailAsync(_configuration["MasterAdmin:Email"]);

            if (user == null)
            {
                user = new Admin
                {
                    FirstName = _configuration["MasterAdmin:FirstName"],
                    LastName = _configuration["MasterAdmin:LastName"],
                    Email = _configuration["MasterAdmin:Email"],
                    UserName = _configuration["MasterAdmin:Email"],
                    PhoneNumber = _configuration["MasterAdmin:PhoneNumber"],
                };

                var result = await _userHelper.AddUserAsync(user, _configuration["MasterAdmin:Password"]);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Could not create the MasterAdmin in seeder!");
                }

                await _userHelper.AddUserToRoleAsync(user, "MasterAdmin");

                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);
            }
        }
    }
}
