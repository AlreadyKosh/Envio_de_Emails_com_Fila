using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Envio_de_Emails_Com_Fila_Worker.Helper
{
    public static class CepHelper
    {
        public static string? ExtrairCep(string texto)
        {
            var match = Regex.Match(texto, @"\b\d{5}-?\d{3}\b");
            return match.Success ? match.Value.Replace("-", "") : null;
        }
    }
}
