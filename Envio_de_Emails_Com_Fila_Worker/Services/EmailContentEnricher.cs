using Envio_de_Emails_Com_Fila_Worker.Helper;
using Envio_de_Emails_Com_Fila_Worker.Services.Interfaces;

namespace Envio_de_Emails_Com_Fila_Worker.Services
{
    public class EmailContentEnricher : IEmailContentEnricher
    {
        private readonly ICepService _cepService;

        public EmailContentEnricher(ICepService cepService)
        {
            _cepService = cepService;
        }

        public async Task<string> EnrichAsync(string content)
        {
            var zipCodes = CepHelper.ExtractZipCode(content)
                .Distinct()
                .ToList();

            if (!zipCodes.Any())
            {
                return content;
            }

            var tasks = zipCodes.Select(async zipCode =>
            {
                var address = await _cepService.GetAddress(zipCode);
                return new { ZipCode = zipCode, Address = address };
            });

            var results = await Task.WhenAll(tasks);

            var validAddresses = results
                .Where(x => !string.IsNullOrEmpty(x.Address))
                .ToList();

            if (!validAddresses.Any())
            {
                return content;
            }

            var enrichedContent = content + "\n\nEnderecos encontrados:\n";

            for (var i = 0; i < validAddresses.Count; i++)
            {
                var item = validAddresses[i];
                enrichedContent += $"\nEndereco {i + 1} (CEP: {item.ZipCode}):\n{item.Address}\n";
            }

            return enrichedContent;
        }
    }
}
