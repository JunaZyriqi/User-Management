using System.Net;
using System.Net.Mail;

namespace ImbUserManagment2.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;

        public EmailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var from = configuration["EmailSettings:From"];
            var smtpServer = configuration["EmailSettings:SmtpServer"];
            var port = int.Parse(configuration["EmailSettings:Port"] ?? "587");
            var username = configuration["EmailSettings:Username"];
            var password = configuration["EmailSettings:Password"];

            var message = new MailMessage(from, toEmail, subject, body);
            message.IsBodyHtml = true;

            using var client = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
    }
}