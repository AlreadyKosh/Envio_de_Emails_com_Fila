using System.Text.RegularExpressions;

namespace Envio_de_Emails_Com_Fila_PersistenceWorker.Helper
{
    public static class CepHelper
    {
        public static List<string> ExtractZipCode(string text)
        {
            var matches = Regex.Matches(text, @"(?<!\d)\d{5}-?\d{3}(?!\d)");
            return matches.Select(m => m.Value.Replace("-", "")).ToList();
        }
    }
}
