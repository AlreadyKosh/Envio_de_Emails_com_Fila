using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Envio_de_Emails_Com_Fila_Worker.Helper
{
    public static class CepHelper
    {
        public static List<string> ExtractZipCode(string texto)
        {
            var matches = Regex.Matches(texto, @"(?<!\d)\d{5}-?\d{3}(?!\d)");
            return matches.Select(m => m.Value.Replace("-", "")).ToList();
        }
    }
}
