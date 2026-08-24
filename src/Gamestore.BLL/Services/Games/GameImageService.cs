using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.Interfaces.Games;
using Gamestore.Domain.Settings;
using Gamestore.Domain.Shared;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;

namespace Gamestore.BLL.Services.Games;

public class GameImageService : IGameImageService
{
    private readonly BlobContainerClient _blobContainerClient;

    public GameImageService(
        IAzureClientFactory<BlobContainerClient> blobContainerClientFactory,
        IOptions<BlobStorageSettings> blobStorageSettings)
    {
        var settings = blobStorageSettings.Value;

        _blobContainerClient = blobContainerClientFactory.CreateClient(settings.GameContainerName);
    }

    public async Task<DownloadGameImageDto> DownloadByUrlAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNullOrWhiteSpace(fileUrl);

        var fileName = GetFileName(fileUrl);
        var blobClient = _blobContainerClient.GetBlobClient(fileName);

        var downloadInfo = await blobClient.DownloadAsync(cancellationToken);

        using var memoryStream = new MemoryStream();

        await downloadInfo.Value.Content.CopyToAsync(memoryStream, cancellationToken);

        return new DownloadGameImageDto
        {
            FileName = fileName,
            ContentType = downloadInfo.Value.ContentType,
            Content = memoryStream.ToArray(),
        };
    }

    public async Task<string> UploadAsync(string fileName, byte[] fileContent, string contentType, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNullOrWhiteSpace(fileName);
        Guard.AgainstNullOrWhiteSpace(contentType);

        var blobClient = _blobContainerClient.GetBlobClient(fileName);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType,
            },
        };

        using var memoryStream = new MemoryStream(fileContent);

        await blobClient.UploadAsync(memoryStream, uploadOptions, cancellationToken);

        return blobClient.Uri.AbsoluteUri;
    }

    public async Task<bool> DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNullOrWhiteSpace(fileName);

        var blobClient = _blobContainerClient.GetBlobClient(fileName);

        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

        return response.Value;
    }

    private static string GetFileName(string fileUrl)
    {
        return Path.GetFileName(new Uri(fileUrl).LocalPath);
    }
}