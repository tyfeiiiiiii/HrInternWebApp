using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using HrInternWebApp.Entity;

namespace HrInternWebApp.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            var client = new SmtpClient(_settings.SmtpServer, int.Parse(_settings.Port))
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            client.Send(mailMessage);
        }
    }
}
