using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.DTOs.Games.Platforms;

namespace Gamestore.BLL.Interfaces.Games;

public interface IPlatformService
{
    Task<IReadOnlyList<PlatformDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GameDto>> GetPlatformGamesAsync(Guid platformId, CancellationToken cancellationToken = default);

    Task<PlatformDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CreatePlatformResponse> CreateAsync(CreatePlatformRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdatePlatformRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}