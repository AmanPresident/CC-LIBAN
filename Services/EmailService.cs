using System.Net;
using System.Net.Mail;

namespace test7.Services
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string email, string username, string token);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailConfirmationAsync(string email, string username, string token)
        {
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"]);
            var smtpEmail = _configuration["Smtp:Email"];
            var smtpPassword = _configuration["Smtp:Password"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpEmail, smtpPassword)
            };

            var confirmationUrl = $"http://localhost:5216/Account/ConfirmEmail?token={token}&email={Uri.EscapeDataString(email)}";

            var message = new MailMessage
            {
                From = new MailAddress(smtpEmail, "DASHMIN App"),
                Subject = "Confirmation de votre compte DASHMIN",
                Body = $@"
                    <html>
                    <body>
                        <h2>Bienvenue {username} !</h2>
                        <p>Merci de vous être inscrit sur DASHMIN.</p>
                        <p>Pour activer votre compte, veuillez cliquer sur le lien ci-dessous :</p>
                        <p><a href='{confirmationUrl}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Confirmer mon compte</a></p>
                        <p>Ce lien expire dans 24 heures.</p>
                        <p>Si vous n'avez pas créé de compte, ignorez cet email.</p>
                    </body>
                    </html>",
                IsBodyHtml = true
            };

            message.To.Add(email);
            await client.SendMailAsync(message);
        }
    }
}