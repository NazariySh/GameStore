using Gamestore.BLL.FilterPipelines.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.FilterPipelines.Implementations.GameFilters;

public class FilterByPlatformsStep : IFilterPipelineStep<Game>
{
    private readonly ICollection<Guid> _platforms;

    public FilterByPlatformsStep(ICollection<Guid> platforms)
    {
        _platforms = platforms;
    }

    public IQueryable<Game> Apply(IQueryable<Game> query)
    {
        return _platforms.Count > 0
            ? query.Where(game => game.GamePlatforms.Any(gp => _platforms.Contains(gp.PlatformId)))
            : query;
    }
}