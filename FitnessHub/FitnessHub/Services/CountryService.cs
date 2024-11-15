using Newtonsoft.Json;
using FitnessHub.Data.Classes;
using FitnessHub.Data.HelperClasses;

namespace FitnessHub.Services
{
    public class CountryService
    {
        private readonly HttpClient _httpClient;

        public CountryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Response> GetCountriesAsync()
        {
            var response = await _httpClient.GetAsync("https://countryinfoapi.com/api/countries");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var countries = JsonConvert.DeserializeObject<List<CountryApi>>(result);

                List<CountryApi> countryList = new List<CountryApi>();

                foreach (var country in countries)
                {
                    if (!string.IsNullOrEmpty(country.Callingcode) && country.Callingcode.ToLower() != "undefined")
                    {
                        countryList.Add(country);
                    }
                }

                return new Response
                {
                    IsSuccess = true,
                    Results = countryList.OrderBy(c => c.Name).ToList()
                };
            }

            return new Response
            {
                IsSuccess = false,
                Message = result
            };
        }
    }
}
