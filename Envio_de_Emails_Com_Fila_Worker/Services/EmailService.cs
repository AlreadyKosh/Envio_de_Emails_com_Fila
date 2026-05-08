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

            var zipCodes = CepHelper.ExtractZipCode(content)
                                .Distinct()
                                .ToList();

            if (zipCodes.Any())
            {
                var tasks = zipCodes.Select(async zipCode =>
                {
                    var address = await _cepService.GetAdress(zipCode);
                    return new { ZipCode = zipCode, Adress = address };
                });

                var results = await Task.WhenAll(tasks);

                var validAddresses = results
                    .Where(x => !string.IsNullOrEmpty(x.Adress))
                    .ToList();

                if (validAddresses.Any())
                {
                    content += "\n\nEndereços encontrados:\n";

                    int i = 1;
                    foreach (var item in validAddresses)
                    {
                        content += $"\nEndereço {i} (CEP: {item.ZipCode}):\n{item.Adress}\n";
                        i++;
                    }
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