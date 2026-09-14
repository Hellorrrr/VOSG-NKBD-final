using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace VOSG_NKBD.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var settings = _config.GetSection("EmailSettings");

            // wrapped these in using - SmtpClient and MailMessage both hold onto
            // unmanaged resources and were never getting disposed before
            using var client = new SmtpClient(settings["SmtpServer"])
            {
                Port = int.Parse(settings["Port"]!),
                Credentials = new NetworkCredential(settings["Username"], settings["Password"]),
                EnableSsl = true,
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(settings["From"]!, "VOSG-NKBD"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };
            mail.To.Add(email);

            await client.SendMailAsync(mail);
        }
    }
}