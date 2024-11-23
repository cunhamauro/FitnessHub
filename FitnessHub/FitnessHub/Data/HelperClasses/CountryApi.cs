namespace FitnessHub.Data.HelperClasses
{
    public class CountryApi
    {
        public string? Name { get; set; }

        public string? Cca2 { get; set; }

        public string? Flag { get; set; }

        public string? Callingcode { get; set; }

        public string? Data => $"{Name} ({Callingcode})";

        //private string CountryCodeToEmoji(string countryCode)
        //{
        //    return string.Concat(countryCode.ToUpper().Select(ch => char.ConvertFromUtf32(ch + 0x1F1A5)));
        //}
    }
}
