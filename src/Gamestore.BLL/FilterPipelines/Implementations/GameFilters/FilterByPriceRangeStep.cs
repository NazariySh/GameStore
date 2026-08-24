using Gamestore.BLL.FilterPipelines.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.FilterPipelines.Implementations.GameFilters;

public class FilterByPriceRangeStep : IFilterPipelineStep<Game>
{
    private readonly decimal? _minPrice;
    private readonly decimal? _maxPrice;

    public FilterByPriceRangeStep(decimal? minPrice, decimal? maxPrice)
    {
        _minPrice = minPrice;
        _maxPrice = maxPrice;
    }

    public IQueryable<Game> Apply(IQueryable<Game> query)
    {
        if (_minPrice.HasValue)
        {
            query = query.Where(game => game.Price >= _minPrice.Value);
        }

        if (_maxPrice.HasValue)
        {
            query = query.Where(game => game.Price <= _maxPrice.Value);
        }

        return query;
    }
}