using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using FitnessHub.Data.Entities.Users;
using FitnessHub.Models;

namespace FitnessHub.Helpers
{
    public interface IUserHelper
    {
        Task<User?> GetUserByEmailAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string password);

        Task<List<T>> GetUsersByTypeAsync<T>() where T : User;

        Task<SignInResult> LoginAsync(LoginViewModel model);

        Task<bool> CheckPasswordAsync(User user, string password);

        Task<object?> GetUserImageAsync(string email);

        Task LogoutAsync();

        Task<IdentityResult> UpdateUserAsync(User user);

        Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword);

        Task CheckRoleAsync(string roleName);

        Task AddUserToRoleAsync(User user, string roleName);

        Task<bool> IsUserInRoleAsync(User user, string roleName);

        Task<SignInResult> ValidatePasswordAsync(User user, string password);

        Task<string> GenerateEmailConfirmationTokenAsync(User user);

        Task<IdentityResult> ConfirmEmailAsync(User user, string token);

        Task<User?> GetUserByIdAsync(string userId);

        Task<string> GeneratePasswordResetTokenAsync(User user);

        Task<IdentityResult> ResetPasswordAsync(User user, string token, string password);

        Task<User?> GetUserAsync(ClaimsPrincipal claims);

        Task<IList<User>> GetAdminsAsync();

        Task<IList<User>> GetEmployeesAndInstructorsAndClientsByGymAsync(int gymId);

        Task<IList<User>> GetClientsByGymAsync(int gymId);

        Task<IList<User>> GetInstructorsByGymAsync(int gymId);

        Task<IList<string>> GetUserRolesAsync(User user);

        IQueryable<IdentityRole> GetAllRoles();

        IQueryable<IdentityRole> GetAdminRoles();

        IQueryable<IdentityRole> GetRolesExceptAdmin();

        Task<IdentityResult> DeleteUser(User user);

        bool CheckIfPhoneNumberExists(string phoneNumber);

        Task<Client?> GetClientIncludeAsync(string id);
      
        Task<int> ClientsWithMembershipCountAsync();

        Task<string> GymWithMostMembershipsAsync();
    }
}
