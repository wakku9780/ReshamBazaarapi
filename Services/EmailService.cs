using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ReshamBazaar.Api.DTOs;

namespace ReshamBazaar.Api.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(EmailRequest emailRequest)
        {
            try
            {
                Console.WriteLine($"[EmailService] Attempting to send email to: {emailRequest.ToEmail}");
                Console.WriteLine($"[EmailService] SMTP Server: {_emailSettings.SmtpServer}:{_emailSettings.Port}");
                Console.WriteLine($"[EmailService] Sender: {_emailSettings.SenderEmail}");

                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, "ReshamBazaar"),
                    Subject = emailRequest.Subject,
                    Body = emailRequest.Body,
                    IsBodyHtml = emailRequest.IsBodyHtml
                };

                mailMessage.To.Add(emailRequest.ToEmail);

                Console.WriteLine("[EmailService] Sending email...");
                await client.SendMailAsync(mailMessage);
                Console.WriteLine("[EmailService] Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] Error sending email: {ex.Message}");
                Console.WriteLine($"[EmailService] Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[EmailService] Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"[EmailService] Inner Stack Trace: {ex.InnerException.StackTrace}");
                }
                throw; // Re-throw to be handled by the caller
            }
        }
    }
}
