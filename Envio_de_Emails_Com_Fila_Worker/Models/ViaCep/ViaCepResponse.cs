using System;
using System.Collections.Generic;
using System.Text;

namespace Envio_de_Emails_Com_Fila_Worker.Models.ViaCep
{
    public class ViaCepResponse
    {
        public string Logradouro { get; set; }
        public string Bairro { get; set; }
        public string Localidade { get; set; }
        public string Uf { get; set; }
        public bool Erro { get; set; }
    }
}
