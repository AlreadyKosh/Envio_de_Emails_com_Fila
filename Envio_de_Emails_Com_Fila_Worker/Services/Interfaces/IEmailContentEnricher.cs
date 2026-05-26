namespace Envio_de_Emails_Com_Fila_Worker.Services.Interfaces
{
    public interface IEmailContentEnricher
    {
        Task<string> EnrichAsync(string content);
    }
}
