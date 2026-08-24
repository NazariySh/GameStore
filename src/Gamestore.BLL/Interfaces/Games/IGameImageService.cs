using Gamestore.BLL.DTOs.Games;

namespace Gamestore.BLL.Interfaces.Games;

public interface IGameImageService
{
    Task<DownloadGameImageDto> DownloadByUrlAsync(string fileUrl, CancellationToken cancellationToken = default);

    Task<string> UploadAsync(string fileName, byte[] fileContent, string contentType, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string fileName, CancellationToken cancellationToken = default);
}