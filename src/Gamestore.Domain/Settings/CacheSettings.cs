namespace Gamestore.Domain.Settings;

public record CacheSettings
{
    public const string SectionName = nameof(CacheSettings);

    public int ExpirationInMinutes { get; init; }

    public int OutputCacheExpirationInMinutes { get; init; }
}