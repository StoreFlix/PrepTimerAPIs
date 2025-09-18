using System.Net.Mail;
using System.Net;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;

namespace PrepTimerAPIs.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                string host = _configuration["SMTP:SmtpServer"];
                int port = Convert.ToInt32(_configuration["SMTP:SmtpPort"]);
                string from = _configuration["SMTP:FromEmail"];
                string password = _configuration["SMTP:EmailPassword"];



                // Messages will be sent from StoreLynk Support for StoreLynk.
                var client = new SmtpClient(host, port)
                { EnableSsl = true, UseDefaultCredentials = false, Credentials = new NetworkCredential(from, password) };
                var message = new MailMessage(from, to, subject, body) { IsBodyHtml = true };
                message.From = new MailAddress("support@storelynk.com", "StoreLynk Support");

                client.Send(message);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
