using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.DTOs.Games.Genres;

namespace Gamestore.BLL.Interfaces.Games;

public interface IGenreService
{
    Task<IReadOnlyList<GenreDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GenreDto>> GetSubGenresAsync(string parentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GameDto>> GetGenreGamesAsync(string genreId, CancellationToken cancellationToken = default);

    Task<GenreDetailedDto> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<CreateGenreResponse> CreateAsync(CreateGenreRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateGenreRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}