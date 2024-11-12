using FitnessHub.Data.Entities;
using FitnessHub.Data.Entities.GymClasses;
using FitnessHub.Data.Entities.GymMachines;
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
            await _context.Machines.AddAsync(noMachine);

            var user = await _userHelper.GetUserByEmailAsync(_configuration["MasterAdmin:Email"]);

            #region TESTES / DESENVOLVIMENTO => APAGAR ANTES DE DEPLOY

            var gym = new Gym
            {
                Name = "GymTest1",
                Country = "North Korea",
                City = "PyongYang",
                Address = "Rua do KimJongUn"
            };

            var gym2 = new Gym
            {
                Name = "GymTest2",
                Country = "Russia",
                City = "Moscow",
                Address = "Rua do Putin"
            };

            await _context.Gyms.AddAsync(gym);
            await _context.Gyms.AddAsync(gym2);

            var category = new MachineCategory
            {
                Name = "Chest",
                Description = "Maminhas",
            };

            var category2 = new MachineCategory
            {
                Name = "Legs",
                Description = "Coxinhas",
            };

            await _context.MachineCategories.AddAsync(category);
            await _context.MachineCategories.AddAsync(category2);

            var categoryC1 = new ClassCategory
            {
                Name = "Yoga",
                Description = "Learn to fuck better",
            };

            var categoryC2 = new ClassCategory
            {
                Name = "Muscles",
                Description = "Learn to take some steroids",
            };

            await _context.ClassCategories.AddAsync(categoryC1);
            await _context.ClassCategories.AddAsync(categoryC2);

            var machine1 = new Machine
            {
                Name = "BenchPress",
                Category = category,
            };

            await _context.Machines.AddAsync(machine1);

            var machine2 = new Machine
            {
                Name = "Squats",
                Category = category2,
            };

            await _context.Machines.AddAsync(machine2);

            var admin = new Admin
            {
                Gym = gym,
                FirstName = "Admin",
                LastName = "Test",
                Email = "admintest@fhub.com",
                UserName = "admintest@fhub.com",
                PhoneNumber = "999999999",
            };

            var result1 = await _userHelper.AddUserAsync(admin, "Teste_1234");
            await _userHelper.AddUserToRoleAsync(admin, "Admin");
            var token1 = await _userHelper.GenerateEmailConfirmationTokenAsync(admin);
            await _userHelper.ConfirmEmailAsync(admin, token1);

            var employee = new Employee
            {
                Gym = gym,
                FirstName = "Employee",
                LastName = "Test",
                Email = "employeetest@fhub.com",
                UserName = "employeetest@fhub.com",
                PhoneNumber = "888888888"
            };

            var result2 = await _userHelper.AddUserAsync(employee, "Teste_1234");
            await _userHelper.AddUserToRoleAsync(employee, "Employee");
            var token2 = await _userHelper.GenerateEmailConfirmationTokenAsync(employee);
            await _userHelper.ConfirmEmailAsync(employee, token2);

            var instructor = new Instructor
            {
                Gym = gym,
                FirstName = "Instructor",
                LastName = "Test",
                Email = "instructortest@fhub.com",
                UserName = "instructortest@fhub.com",
                PhoneNumber = "77777777"
            };

            var result3 = await _userHelper.AddUserAsync(instructor, "Teste_1234");
            await _userHelper.AddUserToRoleAsync(instructor, "Instructor");
            var token3 = await _userHelper.GenerateEmailConfirmationTokenAsync(instructor);
            await _userHelper.ConfirmEmailAsync(instructor, token3);

            var client = new Client
            {
                Gym = gym,
                FirstName = "Client",
                LastName = "Test",
                Email = "clienttest@fhub.com",
                UserName = "clienttest@fhub.com",
                PhoneNumber = "666666666"
            };

            var result4 = await _userHelper.AddUserAsync(client, "Teste_1234");
            await _userHelper.AddUserToRoleAsync(client, "Client");
            var token4 = await _userHelper.GenerateEmailConfirmationTokenAsync(client);
            await _userHelper.ConfirmEmailAsync(client, token4);

            var client2 = new Client
            {
                Gym = gym2,
                FirstName = "Client2",
                LastName = "Test2",
                Email = "clienttest2@fhub.com",
                UserName = "clienttest2@fhub.com",
                PhoneNumber = "666666666"
            };

            var result5 = await _userHelper.AddUserAsync(client2, "Teste_1234");
            await _userHelper.AddUserToRoleAsync(client2, "Client");
            var token5 = await _userHelper.GenerateEmailConfirmationTokenAsync(client2);
            await _userHelper.ConfirmEmailAsync(client2, token5);

            #endregion

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
