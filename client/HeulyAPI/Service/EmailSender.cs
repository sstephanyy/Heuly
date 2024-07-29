using HeulyAPI.Interfaces;
using HeulyAPI.Models;
using System.Net.Mail;
using System.Net;

namespace HeulyAPI.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;

        public EmailSender(EmailConfiguration emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_emailConfig.From),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(to);

            using (var smtpClient = new SmtpClient(_emailConfig.SmtpServer, _emailConfig.Port))
            {
                smtpClient.Credentials = new NetworkCredential(_emailConfig.Username, _emailConfig.Password);
                smtpClient.EnableSsl = true;

                try
                {
                    await smtpClient.SendMailAsync(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro ao enviar e-mail: " + ex.Message);
                }
            }
        }

    }
}
