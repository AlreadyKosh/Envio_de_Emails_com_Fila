using Envio_de_Emails_Com_Fila_Shared.Models;
using Envio_de_Emails_Com_Fila_Worker.Models.Email;
using Envio_de_Emails_Com_Fila_Worker.Services.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils;

namespace Envio_de_Emails_Com_Fila_Worker.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailContentEnricher _emailContentEnricher;
        private readonly EmailSettings _settings;

        public EmailService(IEmailContentEnricher emailContentEnricher, IOptions<EmailSettings> settings)
        {
            _emailContentEnricher = emailContentEnricher;
            _settings = settings.Value;
        }

        public async Task SendEmail(EmailMessage msg)
        {
            var content = await _emailContentEnricher.EnrichAsync(msg.Content);

            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse("test@mailtrap.io"));
            email.To.Add(MailboxAddress.Parse(msg.To));
            email.Subject = "Email Teste";
            email.MessageId = string.IsNullOrWhiteSpace(msg.MessageId)
                ? MimeUtils.GenerateMessageId()
                : $"{msg.MessageId}@envio-emails.local";

            email.Body = new TextPart("plain")
            {
                Text = content
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_settings.Host, _settings.Port, false);
            await smtp.AuthenticateAsync(_settings.User, _settings.Pass);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
