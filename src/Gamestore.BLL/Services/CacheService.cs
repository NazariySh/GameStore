using Gamestore.BLL.Interfaces;
using Gamestore.Domain.Settings;
using Gamestore.Domain.Shared;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Gamestore.BLL.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly CacheSettings _cacheSettings;

    public CacheService(
        IMemoryCache cache,
        IOptions<CacheSettings> cacheSettings)
    {
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
    }

    public Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNullOrWhiteSpace(key);

        return _cache.GetOrCreateAsync(key, entry =>
        {
            entry.SetOptions(new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_cacheSettings.ExpirationInMinutes),
            });

            return factory(cancellationToken);
        });
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNullOrWhiteSpace(key);

        _cache.Remove(key);
        return Task.CompletedTask;
    }
}