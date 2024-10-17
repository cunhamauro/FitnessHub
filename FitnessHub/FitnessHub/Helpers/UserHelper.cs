﻿using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Models;

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

        public Task<IdentityResult> DeleteUser(User user)
        {
            return _userManager.DeleteAsync(user);
        }
    }
}
