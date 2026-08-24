namespace Gamestore.Domain.Settings;

public record RetrySettings
{
    public int MaxRetries { get; init; }

    public int DelayInMilliseconds { get; init; }
}