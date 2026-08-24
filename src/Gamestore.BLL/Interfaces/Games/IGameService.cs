using Gamestore.BLL.DTOs;
using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.DTOs.Games.Genres;
using Gamestore.BLL.DTOs.Games.Platforms;
using Gamestore.BLL.DTOs.Games.Publishers;

namespace Gamestore.BLL.Interfaces.Games;

public interface IGameService
{
    Task<PagedGameList<GameDto>> GetAllFilteredAsync(GameQueryDto query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GameDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GenreDto>> GetGameGenresAsync(string gameKey, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlatformDto>> GetGamePlatformsAsync(string gameKey, CancellationToken cancellationToken = default);

    Task<PublisherDto> GetGamePublisherAsync(string gameKey, CancellationToken cancellationToken = default);

    Task<DownloadGameImageDto> GetGameImageAsync(string gameKey, CancellationToken cancellationToken = default);

    Task<GameDto> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<GameDto> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    Task<DownloadFileContentDto> DownloadFileAsync(string gameKey, CancellationToken cancellationToken = default);

    Task<CreateGameResponse> CreateAsync(CreateGameRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateGameRequest request, CancellationToken cancellationToken = default);

    Task DeleteByKeyAsync(string key, CancellationToken cancellationToken = default);
}