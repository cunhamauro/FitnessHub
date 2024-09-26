using FitnessHub.Data.Classes;

namespace FitnessHub.Helpers
{
    public interface IMailHelper
    {
        Response SendEmail(string to, string subject, string body);
    }
}
