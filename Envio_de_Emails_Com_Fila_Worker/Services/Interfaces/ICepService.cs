using Envio_de_Emails_Com_Fila.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Envio_de_Emails_Com_Fila_Worker.Services.Interfaces
{
    public interface ICepService
    {
        Task<string?> ObterEndereco(string cep);
    }
}
