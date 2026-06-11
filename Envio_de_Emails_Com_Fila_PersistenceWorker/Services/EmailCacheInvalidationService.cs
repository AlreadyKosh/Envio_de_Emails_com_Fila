using StackExchange.Redis;

namespace Envio_de_Emails_Com_Fila_PersistenceWorker.Services
{
    public class EmailCacheInvalidationService
    {
        private readonly IConnectionMultiplexer _redis;

        public EmailCacheInvalidationService(IConfiguration configuration)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("Redis__ConnectionString")
                ?? configuration["Redis:ConnectionString"]
                ?? "localhost:6379";

            _redis = ConnectionMultiplexer.Connect(connectionString);
        }

        public async Task InvalidateAsync(IEnumerable<string> zipCodes)
        {
            var database = _redis.GetDatabase();
            var keysToDelete = new List<RedisKey> { "cache:geral" };
            keysToDelete.AddRange(zipCodes.Distinct().Select(zipCode => (RedisKey)$"cache:cep:{zipCode}"));

            await database.KeyDeleteAsync(keysToDelete.ToArray());
        }
    }
}
