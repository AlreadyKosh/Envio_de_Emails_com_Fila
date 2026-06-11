using Envio_de_Emails_Com_Fila_PersistenceWorker.Models;
using Envio_de_Emails_Com_Fila_PersistenceWorker.Helper;
using Envio_de_Emails_Com_Fila_Shared.Models;
using MongoDB.Driver;

namespace Envio_de_Emails_Com_Fila_PersistenceWorker.Services
{
    public class EmailPersistenceService
    {
        private readonly IMongoCollection<PersistedEmailMessage> _collection;
        private readonly EmailContentEnricher _emailContentEnricher;
        private readonly EmailCacheInvalidationService _cacheInvalidationService;

        public EmailPersistenceService(
            IConfiguration configuration,
            EmailContentEnricher emailContentEnricher,
            EmailCacheInvalidationService cacheInvalidationService)
        {
            _emailContentEnricher = emailContentEnricher;
            _cacheInvalidationService = cacheInvalidationService;

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

        public async Task SaveAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            var messageId = string.IsNullOrWhiteSpace(message.MessageId)
                ? Guid.NewGuid().ToString("N")
                : message.MessageId;

            var content = await _emailContentEnricher.EnrichAsync(message.Content);
            var zipCodes = CepHelper.ExtractZipCode(message.Content).Distinct().ToList();
            var filter = Builders<PersistedEmailMessage>.Filter.Eq(x => x.MessageId, messageId);
            var existingMessage = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            var zipCodesToInvalidate = zipCodes
                .Concat(existingMessage?.ZipCodes ?? [])
                .Distinct()
                .ToList();

            var update = Builders<PersistedEmailMessage>.Update
                .Set(x => x.MessageId, messageId)
                .Set(x => x.To, message.To)
                .Set(x => x.Content, content)
                .Set(x => x.ZipCodes, zipCodes)
                .SetOnInsert(x => x.CreatedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(
                filter,
                update,
                new UpdateOptions { IsUpsert = true },
                cancellationToken
            );

            await _cacheInvalidationService.InvalidateAsync(zipCodesToInvalidate);
        }

        private void CreateIndexes()
        {
            var messageIdKeys = Builders<PersistedEmailMessage>.IndexKeys.Ascending(x => x.MessageId);
            var messageIdOptions = new CreateIndexOptions<PersistedEmailMessage>
            {
                Unique = true,
                PartialFilterExpression = Builders<PersistedEmailMessage>.Filter.Gt(x => x.MessageId, string.Empty)
            };

            var zipCodeKeys = Builders<PersistedEmailMessage>.IndexKeys.Ascending(x => x.ZipCodes);

            _collection.Indexes.CreateMany([
                new CreateIndexModel<PersistedEmailMessage>(messageIdKeys, messageIdOptions),
                new CreateIndexModel<PersistedEmailMessage>(zipCodeKeys)
            ]);
        }
    }
}
