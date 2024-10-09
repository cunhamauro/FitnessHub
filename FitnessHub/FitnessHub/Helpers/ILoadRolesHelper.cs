using FitnessHub.Models;

namespace FitnessHub.Helpers
{
    public interface ILoadRolesHelper
    {
        void LoadMasterAdminRoles(AdminRegisterNewUserViewModel model);

        void LoadAdminRoles(AdminRegisterNewUserViewModel model);
    }
}
