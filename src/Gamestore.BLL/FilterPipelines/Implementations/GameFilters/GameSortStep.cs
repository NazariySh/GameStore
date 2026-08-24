using Gamestore.BLL.Enums;
using Gamestore.BLL.FilterPipelines.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.FilterPipelines.Implementations.GameFilters;

public class GameSortStep : ISortPipelineStep<Game>
{
    private readonly GameSortOption? _sortBy;

    public GameSortStep(GameSortOption? sortBy)
    {
        _sortBy = sortBy;
    }

    public IQueryable<Game> Apply(IQueryable<Game> query)
    {
        return _sortBy?.Name switch
        {
            nameof(GameSortOption.MostPopular) => query.OrderByDescending(g => g.ViewCount),
            nameof(GameSortOption.MostCommented) => query.OrderByDescending(g => g.CommentCount),
            nameof(GameSortOption.PriceAsc) => query.OrderBy(g => g.Price),
            nameof(GameSortOption.PriceDesc) => query.OrderByDescending(g => g.Price),
            nameof(GameSortOption.New) => query.OrderByDescending(g => g.CreatedAt),
            _ => query,
        };
    }
}