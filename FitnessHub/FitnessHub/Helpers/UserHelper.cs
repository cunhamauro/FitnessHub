using FitnessHub.Data.Entities.Users;
using FitnessHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FitnessHub.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserHelper(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<List<T>> GetUsersByTypeAsync<T>() where T : User
        {
            return await _userManager.Users.OfType<T>().ToListAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrWhiteSpace(email) || email == null)
            {
                return null;
            }

            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(
                model.Username, model.Password, model.RememberMe, false);
        }

        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<object?> GetUserImageAsync(string email)
        {
            return await _userManager.Users
                .Where(u => u.Email == email)
                .Select(u => new
                {
                    u.ImagePath,
                })
                .SingleOrDefaultAsync();
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }

        public async Task CheckRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName
                });
            }
        }

        public async Task AddUserToRoleAsync(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<SignInResult> ValidatePasswordAsync(User user, string password)
        {
            return await _signInManager.CheckPasswordSignInAsync(user, password, false);
        }

        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string password)
        {
            return await _userManager.ResetPasswordAsync(user, token, password);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<User?> GetUserAsync(ClaimsPrincipal claims)
        {
            return await _userManager.GetUserAsync(claims);
        }

        public async Task<IList<User>> GetAdminsAsync()
        {
            return await _userManager.GetUsersInRoleAsync("Admin");
        }

        public async Task<IList<User>> GetEmployeesAndInstructorsAndClientsByGymAsync(int gymId)
        {
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor") ?? new List<User>();
            var instructorsByGym = instructors.OfType<Instructor>().Where(i => i.GymId == gymId);

            var employees = await _userManager.GetUsersInRoleAsync("Employee") ?? new List<User>();
            var employeesByGym = employees.OfType<Employee>().Where(i => i.GymId == gymId);

            var clients = await _userManager.GetUsersInRoleAsync("Client") ?? new List<User>();
            var clientsByGym = clients.OfType<Client>().Where(i => i.GymId == gymId);

            var combinedUsers = employeesByGym.Cast<User>()
                .Union(instructorsByGym.Cast<User>())
                .Union(clientsByGym.Cast<User>())
                .ToList();

            return combinedUsers;
        }

        public async Task<IList<User>> GetClientsByGymAsync(int gymId)
        {
            var clients = await _userManager.GetUsersInRoleAsync("Client") ?? new List<User>();
            var clientsByGym = clients.OfType<Client>().Where(i => i.GymId == gymId);

            return clientsByGym.Cast<User>().ToList();
        }

        public async Task<IList<User>> GetInstructorsByGymAsync(int gymId)
        {
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor") ?? new List<User>();
            var instructorsByGym = instructors.OfType<Instructor>().Where(i => i.GymId == gymId);

            return instructorsByGym.Cast<User>().ToList();
        }

        public async Task<IList<string>> GetUserRolesAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public IQueryable<IdentityRole> GetAllRoles()
        {
            return _roleManager.Roles;
        }

        public IQueryable<IdentityRole> GetAdminRoles()
        {
            return _roleManager.Roles
                .Where(r => r.Name == "Admin");
        }

        public IQueryable<IdentityRole> GetRolesExceptAdmin()
        {
            return _roleManager.Roles
                .Where(r => r.Name != "MasterAdmin" && r.Name != "Admin");
        }

        public async Task<IdentityResult> DeleteUser(User user)
        {
            return await _userManager.DeleteAsync(user);
        }

        public bool CheckIfPhoneNumberExists(string phoneNumber)
        {
            return _userManager.Users.Where(u => u.PhoneNumber == phoneNumber).Any();
        }

        public async Task<Client?> GetClientIncludeAsync(string id)
        {
            return await _userManager.Users.OfType<Client>().
                Include(u => u.MembershipDetails).
                Include(u => u.OnlineClass).
                Include(u => u.GymClass).
                Where(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> ClientsWithMembershipCountAsync()
        {
            var clients = await _userManager.GetUsersInRoleAsync("Client") ?? new List<User>();

            return clients.OfType<Client>().Where(u => u.MembershipDetailsId != null).Count();
        }

        public async Task<string> GymWithMostMembershipsAsync()
        {
            var clientsWithMembership = await _userManager.Users
                                       .OfType<Client>()
                                       .Include(c => c.Gym) // Ensure Gym is loaded
                                       .Where(c => c.MembershipDetailsId != null)
                                       .ToListAsync();

            if (!clientsWithMembership.Any())
            {
                return "N/A";
            }

            var gymMemberships = clientsWithMembership
                         .GroupBy(c => c.Gym)
                         .Select(group => new
                         {
                             Gym = group.Key,
                             MembershipCount = group.Count()
                         })
                         .ToList();

            var maxMembershipCount = gymMemberships.Max(g => g.MembershipCount);

            var gymsWithMaxMemberships = gymMemberships
                                 .Where(g => g.MembershipCount == maxMembershipCount)
                                 .Select(g => g.Gym.Name)
                                 .ToList();

            if (gymsWithMaxMemberships.Any())
            {
                return $"{string.Join(", ", gymsWithMaxMemberships)}";
            }
            else
            {
                return "N/A";
            }
        }
     
    }
}
