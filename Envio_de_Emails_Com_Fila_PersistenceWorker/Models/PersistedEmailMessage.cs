using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Envio_de_Emails_Com_Fila_PersistenceWorker.Models
{
    public class PersistedEmailMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string MessageId { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> ZipCodes { get; set; } = [];
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
