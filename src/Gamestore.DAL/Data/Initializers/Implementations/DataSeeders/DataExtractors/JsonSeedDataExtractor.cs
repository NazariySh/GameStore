using System.Text.Json;
using System.Text.Json.Serialization;
using Gamestore.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Gamestore.DAL.Data.Initializers.Implementations.DataSeeders.DataExtractors;

public class JsonSeedDataExtractor : ISeedDataExtractor
{
    private const string FileExtension = ".json";

    private static readonly string SeedDataPath = Path.Combine(AppContext.BaseDirectory, "Data", "SeedData");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
        },
    };

    private readonly ILogger<JsonSeedDataExtractor> _logger;

    public JsonSeedDataExtractor(ILogger<JsonSeedDataExtractor> logger)
    {
        _logger = logger;
    }

    public async Task<List<T>> ExtractAsync<T>(string fileName, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNullOrWhiteSpace(fileName);

        var filePath = GetSeedDataFilePath(fileName);

        if (File.Exists(filePath))
        {
            _logger.LogInformation("Extracting seed data from file: '{FilePath}'", filePath);
            return await GetFileDataAsync<T>(filePath, cancellationToken);
        }

        _logger.LogWarning("Seed data file not found: '{FilePath}'", filePath);
        return [];
    }

    private static async Task<List<T>> GetFileDataAsync<T>(string filePath, CancellationToken cancellationToken)
    {
        var data = await File.ReadAllTextAsync(filePath, cancellationToken);
        return JsonSerializer.Deserialize<List<T>>(data, JsonOptions) ?? [];
    }

    private static string GetSeedDataFilePath(string fileName)
    {
        return Path.Combine(SeedDataPath, GetFileNameWithExtension(fileName));
    }

    private static string GetFileNameWithExtension(string fileName)
    {
        return $"{fileName}{FileExtension}";
    }
}