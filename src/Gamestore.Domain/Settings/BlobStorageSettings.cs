namespace Gamestore.Domain.Settings;

public record BlobStorageSettings
{
    public const string SectionName = "AzureBlobStorage";

    public string ConnectionString { get; init; }

    public string GameContainerName { get; init; }
}