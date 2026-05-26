using System;
using System.Collections.Generic;
using System.Text;

namespace Envio_de_Emails_Com_Fila_Worker.Models.Email
{
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string User { get; set; } = string.Empty;
        public string Pass { get; set; } = string.Empty;
    }
}
