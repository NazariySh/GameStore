namespace Gamestore.Domain.Settings;

public record JwtSettings
{
    public const string SectionName = nameof(JwtSettings);

    public string Key { get; init; }

    public string Issuer { get; init; }

    public string Audience { get; init; }

    public int AccessTokenExpiryInMinutes { get; init; }
}