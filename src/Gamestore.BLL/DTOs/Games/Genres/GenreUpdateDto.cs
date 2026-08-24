using Gamestore.Domain.Shared;

namespace Gamestore.BLL.DTOs.Games.Genres;

public record GenreUpdateDto : GenreCreateUpdateDto
{
    public EntityId Id { get; init; }
}