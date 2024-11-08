using FitnessHub.Data.HelperClasses;
using FitnessHub.Models;

namespace FitnessHub.Helpers
{
    public interface ILoadHelper
    {
        void LoadMasterAdminRoles(AdminRegisterNewUserViewModel model);

        void LoadAdminRoles(AdminRegisterNewUserViewModel model);

        Task<IEnumerable<CountryApi>> LoadCountriesAsync();
    }
}
