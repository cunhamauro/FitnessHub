using Microsoft.AspNetCore.Mvc.Rendering;
using FitnessHub.Models;
using FitnessHub.Services;
using FitnessHub.Data.HelperClasses;

namespace FitnessHub.Helpers
{
    public class LoadHelper : ILoadHelper
    {
        private readonly IUserHelper _userHelper;
        private readonly CountryService _countryService;

        public LoadHelper(IUserHelper userHelper, CountryService countryService)
        {
            _userHelper = userHelper;
            _countryService = countryService;
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

        public async Task<IEnumerable<CountryApi>> LoadCountriesAsync()
        {
            var countriesResult = await _countryService.GetCountriesAsync();

            var countries = (IEnumerable<CountryApi>)countriesResult.Results;

            return countries;
        }
    }
}
