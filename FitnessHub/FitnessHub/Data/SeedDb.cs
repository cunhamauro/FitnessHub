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

            #region TESTES / DESENVOLVIMENTO => APAGAR ANTES DE DEPLOY

            var gym = new Gym
            {
                Name = "GymTest1",
                Country = "North Korea",
                City = "PyongYang",
                Address = "Rua do KimJongUn",
                FlagUrl = "https://flagcdn.com/kp.svg",
            };

            var gymHist = new GymHistory
            {
                Name = "GymTest1",
                Country = "North Korea",
                City = "PyongYang",
                Address = "Rua do KimJongUn",
                Id = 1,
            };

            var gym2 = new Gym
            {
                Name = "GymTest2",
                Country = "Russia",
                City = "Moscow",
                Address = "Rua do Putin",
                FlagUrl = "https://flagcdn.com/ru.svg",
            };

            var gymHist2 = new GymHistory
            {
                Name = "GymTest2",
                Country = "Russia",
                City = "Moscow",
                Address = "Rua do Putin",
                Id = 2,
            };

            if (!_context.Gyms.Any())
            {
                await _context.Gyms.AddAsync(gym);
                await _context.Gyms.AddAsync(gym2);
                await _context.GymsHistory.AddAsync(gymHist);
                await _context.GymsHistory.AddAsync(gymHist2);
            }

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

            if (!_context.MachineCategories.Any())
            {
                await _context.MachineCategories.AddAsync(category);
                await _context.MachineCategories.AddAsync(category2);
            }

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

            if (!_context.ClassCategories.Any())
            {
                await _context.ClassCategories.AddAsync(categoryC1);
                await _context.ClassCategories.AddAsync(categoryC2);
            }

            var classType1 = new ClassType
            {
                Name = "Kamasutra",
                ClassCategory = categoryC1,
                Description = "Aprender a foder",
            };


            var classType2 = new ClassType
            {
                Name = "Ayuwervedra",
                ClassCategory = categoryC1,
                Description = "aprender magia",
            };

            if (!_context.ClassTypes.Any())
            {
                await _context.ClassTypes.AddAsync(classType1);
                await _context.ClassTypes.AddAsync(classType2);
            }

            var membership = new Membership
            {
                Name = "steroidShip",
                Price = 20,
                Description = "Pa ficares bixo e morreres cedo",
            };

            if (!_context.Memberships.Any())
            {
                await _context.Memberships.AddAsync(membership);
            }

            await _context.SaveChangesAsync();

            var membershipHist = new MembershipHistory
            {
                Id = membership.Id,
                Name = "steroidShip",
                Price = 20,
                Description = "Pa ficares bixo e morreres cedo",
                DateCreated = DateTime.Now,
            };

            if (!_context.MembershipHistory.Any())
            {
                await _context.MembershipHistory.AddAsync(membershipHist);
            }

            var machine1 = new Machine
            {
                Name = "BenchPress",
                Category = category,
            };

            var machine2 = new Machine
            {
                Name = "Squats",
                Category = category2,
            };

            if (!_context.MembershipHistory.Any())
            {
                await _context.Machines.AddAsync(machine1);
                await _context.Machines.AddAsync(machine2);

            }

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

            var employeeHist = new StaffHistory
            {
                GymId = 1,
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Role = "Employee",
                PhoneNumber = "77777777"
            };


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

            var instrucHist = new StaffHistory
            {
                GymId = 1,
                Id = instructor.Id,
                FirstName = instructor.FirstName,
                LastName = instructor.LastName,
                Email = instructor.Email,
                Role = "Instructor",
                PhoneNumber = "77777777"
            };

            if (!_context.StaffHistory.Any())
            {
                await _context.StaffHistory.AddAsync(employeeHist);
                await _context.StaffHistory.AddAsync(instrucHist);
            }

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

            var clientHist = new ClientHistory
            {
                Id = client.Id,
                Email = client.Email,
                FirstName = client.FirstName,
                LastName = client.LastName,
                BirthDate = client.BirthDate,
                GymId = 1,
                PhoneNumber = client.PhoneNumber,
            };

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

            var clientHist2 = new ClientHistory
            {
                Id = client2.Id,
                Email = client2.Email,
                FirstName = client2.FirstName,
                LastName = client2.LastName,
                BirthDate = client2.BirthDate,
                GymId = 2,
                PhoneNumber = client2.PhoneNumber,
            };

            if (!_context.ClientsHistory.Any())
            {
                await _context.ClientsHistory.AddAsync(clientHist);
                await _context.ClientsHistory.AddAsync(clientHist2);
            }

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
