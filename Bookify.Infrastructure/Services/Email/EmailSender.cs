using Bookify.Application.Interfaces.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Bookify.Infrastructure.Services.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var host = _configuration["EmailSettings:Host"];
            var portStr = _configuration["EmailSettings:Port"];
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];
            var from = _configuration["EmailSettings:From"];

            // DEBUG LOGS (Safe)
            _logger.LogInformation($"[SMTP DEBUG] Host: '{host}', Port: '{portStr}', User: '{username}', Pass Length: {password?.Length ?? 0}");

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Email sending is NOT configured. Logging email instead.");
                _logger.LogInformation($"[EMAIL] To: {email} | Subject: {subject} | Message: {message}");
                return;
            }

            int port = int.TryParse(portStr, out var p) ? p : 587;

            using var smtpClient = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(from ?? username),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent to {email}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {email}.");
                throw;
            }
        }
    }
}
