using System;
using System.Collections.Generic;
using System.Text;

namespace Envio_de_Emails_Com_Fila_Worker.Models.Email
{
    public class EmailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Pass { get; set; }
    }
}
