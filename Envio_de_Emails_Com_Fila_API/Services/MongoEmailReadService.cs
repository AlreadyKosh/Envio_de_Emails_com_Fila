using Envio_de_Emails_Com_Fila_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Envio_de_Emails_Com_Fila_API.Services
{
    public class MongoEmailReadService
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly EmailCacheService _cacheService;

        public MongoEmailReadService(IConfiguration configuration, EmailCacheService cacheService)
        {
            _cacheService = cacheService;

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
            _collection = database.GetCollection<BsonDocument>("emails");
        }

        public async Task<IReadOnlyList<PersistedEmailMessageResponse>> GetEmailsAsync(
            int limit,
            string? zipCode,
            CancellationToken cancellationToken)
        {
            var safeLimit = Math.Clamp(limit, 1, 200);
            var normalizedZipCode = NormalizeZipCode(zipCode);

            if (!string.IsNullOrWhiteSpace(normalizedZipCode) && normalizedZipCode.Length != 8)
            {
                return [];
            }

            var cacheKey = string.IsNullOrWhiteSpace(normalizedZipCode)
                ? EmailCacheService.GetGeneralKey()
                : EmailCacheService.GetCepKey(normalizedZipCode);

            var cachedEmails = await _cacheService.GetAsync(cacheKey);

            if (cachedEmails != null)
            {
                return cachedEmails.Take(safeLimit).ToList();
            }

            var filter = string.IsNullOrWhiteSpace(normalizedZipCode)
                ? Builders<BsonDocument>.Filter.Empty
                : Builders<BsonDocument>.Filter.Or(
                    Builders<BsonDocument>.Filter.AnyEq("ZipCodes", normalizedZipCode),
                    Builders<BsonDocument>.Filter.Regex("Content", BuildZipCodeRegex(normalizedZipCode))
                );

            var documents = await _collection
                .Find(filter)
                .Sort(Builders<BsonDocument>.Sort.Descending("CreatedAt"))
                .Limit(200)
                .ToListAsync(cancellationToken);

            var emails = documents.Select(MapToResponse).ToList();
            await _cacheService.SetAsync(cacheKey, emails);

            return emails.Take(safeLimit).ToList();
        }

        private static PersistedEmailMessageResponse MapToResponse(BsonDocument document)
        {
            return new PersistedEmailMessageResponse
            {
                Id = document.GetValue("_id", string.Empty).ToString() ?? string.Empty,
                MessageId = document.GetValue("MessageId", string.Empty).AsString,
                To = document.GetValue("To", string.Empty).AsString,
                Content = document.GetValue("Content", string.Empty).AsString,
                ZipCodes = document.GetValue("ZipCodes", new BsonArray()).AsBsonArray.Select(x => x.AsString).ToList(),
                CreatedAt = document.GetValue("CreatedAt", DateTime.MinValue).ToUniversalTime()
            };
        }

        private static string? NormalizeZipCode(string? zipCode)
        {
            if (string.IsNullOrWhiteSpace(zipCode))
            {
                return null;
            }

            return new string(zipCode.Where(char.IsDigit).ToArray());
        }

        private static BsonRegularExpression BuildZipCodeRegex(string zipCode)
        {
            var pattern = $@"(?<!\d){zipCode[..5]}-?{zipCode[5..]}(?!\d)";
            return new BsonRegularExpression(pattern);
        }
    }
}
