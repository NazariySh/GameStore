namespace Gamestore.Domain.Settings;

public record AuthApiSettings
{
    public const string SectionName = "ExternalAuthApi";

    public string BaseUrl { get; init; }
}