namespace Gamestore.Domain.Settings;

public record PaymentSettings
{
    public const string SectionName = "PaymentApi";

    public string BaseUrl { get; init; }

    public RetrySettings RetrySettings { get; init; }
}