using Gamestore.Domain.Shared;

namespace Gamestore.BLL.DTOs.Games;

public abstract record CreateUpdateGameRequest
{
    public ICollection<EntityId> Genres { get; init; }

    public ICollection<Guid> Platforms { get; init; }

    public EntityId Publisher { get; init; }

    public string? Image { get; init; }
}