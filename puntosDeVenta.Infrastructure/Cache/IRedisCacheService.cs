using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace puntosDeVenta.Infrastructure.Cache
{
    public interface IRedisCacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan ttl);
        Task RemoveAsync(string key);
    }

    public class RedisCacheService : IRedisCacheService, IDisposable
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisCacheService(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _db = _redis.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var data = await _db.StringGetAsync(key);
            if (!data.HasValue)
                return default;
            return JsonSerializer.Deserialize<T>(data!);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
        {
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, ttl);
        }

        public async Task RemoveAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }

        public void Dispose()
        {
            _redis?.Dispose();
        }
    }
}