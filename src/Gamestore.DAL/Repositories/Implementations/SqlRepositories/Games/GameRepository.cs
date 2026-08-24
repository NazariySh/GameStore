using System.Linq.Expressions;
using Gamestore.DAL.Data;
using Gamestore.DAL.Repositories.Interfaces.Games;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Constants;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.DAL.Repositories.Implementations.SqlRepositories.Games;

public class GameRepository : Repository<Game>, IGameRepository
{
    private readonly IUserContext _userContext;

    public GameRepository(
        GamestoreDbContext context,
        IUserContext userContext)
        : base(context)
    {
        _userContext = userContext;
    }

    public Task SoftDeleteAsync(Game game, CancellationToken cancellationToken = default)
    {
        game.IsDeleted = true;
        game.ImageUrl = null;
        return UpdateAsync(game, cancellationToken);
    }

    public Task UpdateUnitInStockAsync(Game game, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(game, cancellationToken);
    }

    public Task UpdateViewCountAsync(Game game, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(game, cancellationToken);
    }

    public Task UpdateCommentCountAsync(Game game, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(game, cancellationToken);
    }

    protected override Expression<Func<Game, bool>> SetGlobalFilter()
    {
        return g =>
            !g.IsDeleted ||
            _userContext.HasPermission(Permissions.ViewDeletedGames);
    }
}