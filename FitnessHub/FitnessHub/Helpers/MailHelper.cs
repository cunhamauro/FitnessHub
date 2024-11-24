using MimeKit;
using FitnessHub.Data.Classes;

namespace FitnessHub.Helpers
{
    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration _configuration;

        public MailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetEmailTemplate(string title, string message, string footer_text)
        {
            return $@"<div style=""background-color:#FFFFFF; margin:0; padding:0;"">
                        <table width=""100%"" style=""background-color:#000000;"">
                            <tr>
                                <td align=""center"" style=""padding: 10px;"">
                                    <img src=""https://i.imgur.com/Lnp2Y7x.png"" alt=""Header Image"" style=""max-width:40%; height:auto;"">
                                </td>
                            </tr>
                        </table>
                        <table width=""600"" style=""margin: 0 auto; background-color: #FFFFFF;"">
                            <tr>
                                <td style=""padding: 30px;"">
                                    <h1 style=""font-size: 24px; color: #000;"">{title}</h1>
                                    <p style=""color: #4D4D4D;"">{message}</p>
                                </td>
                            </tr>
                        </table>
                        <table width=""100%"" style=""background-color:#000000;"">
                            <tr>
                                <td align=""center"" style=""padding: 15px;"">
                                    <p style=""color: #FFFFFF; font-weight: bold"">{footer_text}</p>
                                </td>
                            </tr>
                        </table>
                    </div>";
        }

        public async Task<Response> SendEmailAsync(string to, string subject, string body, MemoryStream pdfStream, string pdfName)
        {
            var senderEmail = _configuration["Mail:SenderEmail"];
            var sender = _configuration["Mail:Sender"];
            var smtp = _configuration["Mail:Smtp"];
            var port = _configuration["Mail:Port"];
            var password = _configuration["Mail:Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderEmail, sender));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;

            var bodybuilder = new BodyBuilder
            {
                HtmlBody = body,
            };

            if (pdfStream != null)
            {
                bodybuilder.Attachments.Add(pdfName, pdfStream.ToArray(), new ContentType("application", "pdf"));
            }

            message.Body = bodybuilder.ToMessageBody();

            try
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync(smtp, int.Parse(port), MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(senderEmail, password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.ToString()
                };
            }

            return new Response
            {
                IsSuccess = true,
            };
        }
    }
}
