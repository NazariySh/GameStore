using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Shared;

namespace Gamestore.Domain.Exceptions;

public class GameNotFoundException : NotFoundException
{
    public GameNotFoundException(Guid id)
        : base(nameof(Game), id)
    {
    }

    public GameNotFoundException(EntityId id)
        : base(nameof(Game), id)
    {
    }

    public GameNotFoundException(string key)
        : base($"{nameof(Game)} with Key '{key}' not found.")
    {
    }
}