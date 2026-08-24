using Gamestore.Domain.Entities.Games;

namespace Gamestore.DAL.Repositories.Interfaces.Games;

public interface IGameRepository : IRepository<Game>
{
    Task SoftDeleteAsync(Game game, CancellationToken cancellationToken = default);

    Task UpdateUnitInStockAsync(Game game, CancellationToken cancellationToken = default);

    Task UpdateViewCountAsync(Game game, CancellationToken cancellationToken = default);

    Task UpdateCommentCountAsync(Game game, CancellationToken cancellationToken = default);
}