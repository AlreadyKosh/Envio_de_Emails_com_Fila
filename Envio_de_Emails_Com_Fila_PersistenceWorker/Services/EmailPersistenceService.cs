using Envio_de_Emails_Com_Fila_PersistenceWorker.Models;
using Envio_de_Emails_Com_Fila_Shared.Models;
using MongoDB.Driver;

namespace Envio_de_Emails_Com_Fila_PersistenceWorker.Services
{
    public class EmailPersistenceService
    {
        private readonly IMongoCollection<PersistedEmailMessage> _collection;

        public EmailPersistenceService(IConfiguration configuration)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("MongoDb__ConnectionString")
                ?? configuration["MongoDb:ConnectionString"]
                ?? "mongodb://localhost:27017";

            var databaseName =
                Environment.GetEnvironmentVariable("MongoDb__Database")
                ?? configuration["MongoDb:Database"]
                ?? "EnvioEmailsDb";

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<PersistedEmailMessage>("emails");

            CreateIndexes();
        }

        public Task SaveAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            var messageId = string.IsNullOrWhiteSpace(message.MessageId)
                ? Guid.NewGuid().ToString("N")
                : message.MessageId;

            var filter = Builders<PersistedEmailMessage>.Filter.Eq(x => x.MessageId, messageId);

            var update = Builders<PersistedEmailMessage>.Update
                .SetOnInsert(x => x.MessageId, messageId)
                .SetOnInsert(x => x.To, message.To)
                .SetOnInsert(x => x.Content, message.Content)
                .SetOnInsert(x => x.CreatedAt, DateTime.UtcNow);

            return _collection.UpdateOneAsync(
                filter,
                update,
                new UpdateOptions { IsUpsert = true },
                cancellationToken
            );
        }

        private void CreateIndexes()
        {
            var keys = Builders<PersistedEmailMessage>.IndexKeys.Ascending(x => x.MessageId);
            var options = new CreateIndexOptions<PersistedEmailMessage>
            {
                Unique = true,
                PartialFilterExpression = Builders<PersistedEmailMessage>.Filter.Ne(x => x.MessageId, string.Empty)
            };

            _collection.Indexes.CreateOne(new CreateIndexModel<PersistedEmailMessage>(keys, options));
        }
    }
}
