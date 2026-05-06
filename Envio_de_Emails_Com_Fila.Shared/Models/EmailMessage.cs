using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Envio_de_Emails_Com_Fila.Shared.Models
{
    public class EmailMessage
    {
        public string To { get; set; }
        public string Content { get; set; }
    }
}
