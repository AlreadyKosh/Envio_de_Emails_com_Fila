using System.Diagnostics;

namespace Envio_de_Emails_Com_Fila_Shared.Observability;

public static class EmailQueueActivitySource
{
    public const string Name = "EnvioDeEmails.Queue";

    public static readonly ActivitySource Instance = new(Name);
}
