using Envio_de_Emails_Com_Fila_Worker.Models.ViaCep;
using Envio_de_Emails_Com_Fila_Worker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace Envio_de_Emails_Com_Fila_Worker.Services
{
    public class CepService : ICepService
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
                return null;

            var json = await response.Content.ReadFromJsonAsync<ViaCepResponse>();

            if (json == null || json.Erro == true)
                return null;

            return $"{json.Logradouro}, {json.Bairro}, {json.Localidade} - {json.Uf}";
        }
    }
}
