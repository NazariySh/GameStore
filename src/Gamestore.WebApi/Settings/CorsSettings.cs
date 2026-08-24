namespace Gamestore.WebApi.Settings;

public record CorsSettings
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; init; }

    public string[] AllowedHeaders { get; init; }

    public string[] AllowedMethods { get; init; }

    public string[] ExposedHeaders { get; init; }
}