using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace BigDataForecasting.API.Services.Caching
{
    public class RedisCacheManager : IRedisCachingService
    {
        private readonly IConnectionMultiplexer _redisCon;
        private readonly IDatabase _cache;

        // ŞİFRE BURADA! Tek bir kilit yerine, her Cache Key için özel kilit üreten havuz:
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public RedisCacheManager(IConnectionMultiplexer redisCon)
        {
            _redisCon = redisCon;
            _cache = redisCon.GetDatabase();
        }

        public async Task<T?> GetOrAddAsync<T>(string key, Func<Task<T>> action, TimeSpan? absoluteExpiration = null)
        {
            // 1. Önce Redis'e bak (Kilitlenmeden, hızlıca)
            var cachedData = await _cache.StringGetAsync(key);
            if (!cachedData.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<T>(cachedData!.ToString())!;
            }

            // 2. Veri yoksa, sadece bu "KEY" için özel bir kilit al/oluştur!
            var myLock = _locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));

            await myLock.WaitAsync(); // Odaya sadece 1 kişi girsin
            try
            {
                // İçeri giren kişi bir daha Redis'i kontrol etsin (belki kapıda beklerken başkası yazmıştır)
                cachedData = await _cache.StringGetAsync(key);
                if (!cachedData.IsNullOrEmpty)
                {
                    return JsonSerializer.Deserialize<T>(cachedData!.ToString())!;
                }

                // Hala yoksa, o ağır SQL/ML işlemini çalıştır (action)
                var result = await action();

                // Sonucu Redis'e kaydet
                if (result != null)
                {
                    var serializedData = JsonSerializer.Serialize(result);
                    await _cache.StringSetAsync(key, serializedData, absoluteExpiration ?? TimeSpan.FromHours(1));
                }

                return result;
            }
            finally
            {
                // İşin bitince sadece kendi kilidini serbest bırak
                myLock.Release();
            }
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.KeyDeleteAsync(key);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null)
        {
            var serializedData = JsonSerializer.Serialize(value);
            await _cache.StringSetAsync(key, serializedData, absoluteExpiration ?? TimeSpan.FromHours(1));
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var cachedData = await _cache.StringGetAsync(key);
            if (!cachedData.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<T>(cachedData!.ToString())!;
            }
            return default;
        }
    }
}