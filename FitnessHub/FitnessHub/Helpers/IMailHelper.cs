using FitnessHub.Data.Classes;

namespace FitnessHub.Helpers
{
    public interface IMailHelper
    {
        Task<Response> SendEmailAsync(string to, string subject, string body, MemoryStream pdfStream, string pdfName);

        string GetEmailTemplate(string title, string message, string footer_text);
    }
}
