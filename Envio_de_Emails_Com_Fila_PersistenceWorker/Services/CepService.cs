using Envio_de_Emails_Com_Fila_PersistenceWorker.Models.ViaCep;
using System.Net.Http.Json;

namespace Envio_de_Emails_Com_Fila_PersistenceWorker.Services
{
    public class CepService
    {
        private readonly HttpClient _http;

        public CepService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string?> GetAddress(string zipCode)
        {
            var response = await _http.GetAsync($"https://viacep.com.br/ws/{zipCode}/json/");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<ViaCepResponse>();

            if (json == null || json.Erro)
            {
                return null;
            }

            return $"{json.Logradouro}, {json.Bairro}, {json.Localidade} - {json.Uf}";
        }
    }
}
