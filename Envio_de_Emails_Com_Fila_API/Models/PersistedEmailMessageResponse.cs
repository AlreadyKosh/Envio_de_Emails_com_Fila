namespace Envio_de_Emails_Com_Fila_API.Models
{
    public class PersistedEmailMessageResponse
    {
        public string Id { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> ZipCodes { get; set; } = [];
        public DateTime CreatedAt { get; set; }
    }
}
