using System.Text.Json;
using Envio_de_Emails_Com_Fila_API.Models;
using StackExchange.Redis;

namespace Envio_de_Emails_Com_Fila_API.Services
{
    public class EmailCacheService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private readonly IDatabase _database;

        public EmailCacheService(IConfiguration configuration)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("Redis__ConnectionString")
                ?? configuration["Redis:ConnectionString"]
                ?? "localhost:6379";

            var redis = ConnectionMultiplexer.Connect(connectionString);
            _database = redis.GetDatabase();
        }

        public static string GetGeneralKey()
        {
            return "cache:geral";
        }

        public static string GetCepKey(string zipCode)
        {
            return $"cache:cep:{NormalizeZipCode(zipCode)}";
        }

        public async Task<IReadOnlyList<PersistedEmailMessageResponse>?> GetAsync(string key)
        {
            var cachedValue = await _database.StringGetAsync(key);

            if (!cachedValue.HasValue)
            {
                return null;
            }

            return JsonSerializer.Deserialize<List<PersistedEmailMessageResponse>>(cachedValue.ToString());
        }

        public Task SetAsync(string key, IReadOnlyList<PersistedEmailMessageResponse> emails)
        {
            var value = JsonSerializer.Serialize(emails);
            return _database.StringSetAsync(key, value, CacheDuration);
        }

        private static string NormalizeZipCode(string zipCode)
        {
            return new string(zipCode.Where(char.IsDigit).ToArray());
        }
    }
}
