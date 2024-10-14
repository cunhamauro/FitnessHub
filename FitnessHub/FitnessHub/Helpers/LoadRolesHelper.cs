using Microsoft.AspNetCore.Mvc.Rendering;
using FitnessHub.Models;

namespace FitnessHub.Helpers
{
    public class LoadRolesHelper : ILoadRolesHelper
    {
        private readonly IUserHelper _userHelper;

        public LoadRolesHelper(IUserHelper userHelper)
        {
            _userHelper = userHelper;
        }

        public void LoadMasterAdminRoles(AdminRegisterNewUserViewModel model)
        {
            model.Roles = _userHelper.GetAdminRoles().Select(role => new SelectListItem
            {
                Value = role.Name,
                Text = role.Name,
            }).ToList();
        }

        public void LoadAdminRoles(AdminRegisterNewUserViewModel model)
        {
            model.Roles = _userHelper.GetRolesExceptAdmin().Select(role => new SelectListItem
            {
                Value = role.Name,
                Text = role.Name,
            }).ToList();
        }
    }
}
