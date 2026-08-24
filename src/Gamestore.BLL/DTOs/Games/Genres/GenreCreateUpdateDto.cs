using Gamestore.Domain.Shared;

namespace Gamestore.BLL.DTOs.Games.Genres;

public abstract record GenreCreateUpdateDto
{
    public string Name { get; init; }

    public EntityId? ParentGenreId { get; init; }
}