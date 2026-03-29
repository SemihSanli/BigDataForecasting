namespace BigDataForecasting.API.Services.Caching
{
    public interface IRedisCachingService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T data, TimeSpan? absoluteExpireTime = null);
        Task RemoveAsync(string key);

        Task<T?> GetOrAddAsync<T>(string key, Func<Task<T>> action, TimeSpan? expireTime = null);
      
    }
}
