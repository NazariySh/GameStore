using Gamestore.Domain.Shared;

namespace Gamestore.BLL.DTOs.Games;

public record GameUpdateDto : GameCreateUpdateDto
{
    public EntityId Id { get; init; }

    public string Key { get; init; }
}