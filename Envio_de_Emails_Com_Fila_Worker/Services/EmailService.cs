using Envio_de_Emails_Com_Fila.Shared.Models;
using MailKit.Net.Smtp;
using MimeKit;

namespace Envio_de_Emails_Com_Fila_Worker.Services
{
    public class EmailService
    {
        public async Task SendEmail(EmailMessage msg)
        {
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse("test@mailtrap.io"));
            email.To.Add(MailboxAddress.Parse(msg.To));
            email.Subject = "Email Teste";

            email.Body = new TextPart("plain")
            {
                Text = msg.Content
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync("sandbox.smtp.mailtrap.io", 587, false);
            await smtp.AuthenticateAsync("6b645d262dac7f", "e67e38b7ea2b14");
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}