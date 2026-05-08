using Envio_de_Emails_Com_Fila.Shared.Models;
using Envio_de_Emails_Com_Fila_Worker.Helper;
using Envio_de_Emails_Com_Fila_Worker.Models.Email;
using Envio_de_Emails_Com_Fila_Worker.Services.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Runtime;

namespace Envio_de_Emails_Com_Fila_Worker.Services
{
    public class EmailService : IEmailService
    {
        private readonly CepService _cepService;
        private readonly EmailSettings _settings;

        public EmailService(CepService cepService, IOptions<EmailSettings> settings)
        {
            _cepService = cepService;
            _settings = settings.Value;

        }

        public async Task SendEmail(EmailMessage msg)
        {
            var content = msg.Content;

            var cep = CepHelper.ExtrairCep(content);

            if (!string.IsNullOrEmpty(cep))
            {
                var endereco = await _cepService.ObterEndereco(cep);

                if (!string.IsNullOrEmpty(endereco))
                {
                    content += $"\n\nEndereço encontrado:\n{endereco}";
                }
            }

            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse("test@mailtrap.io"));
            email.To.Add(MailboxAddress.Parse(msg.To));
            email.Subject = "Email Teste";

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