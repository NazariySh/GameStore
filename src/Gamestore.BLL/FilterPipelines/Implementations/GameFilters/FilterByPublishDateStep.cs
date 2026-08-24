using Gamestore.BLL.Enums;
using Gamestore.BLL.FilterPipelines.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.FilterPipelines.Implementations.GameFilters;

public class FilterByPublishDateStep : IFilterPipelineStep<Game>
{
    private readonly PublishDateOption? _publishDate;

    public FilterByPublishDateStep(PublishDateOption? publishDate)
    {
        _publishDate = publishDate;
    }

    public IQueryable<Game> Apply(IQueryable<Game> query)
    {
        if (_publishDate is null)
        {
            return query;
        }

        var targetPublishDate = DateTime.UtcNow.Subtract(_publishDate.PublishPeriod);

        return query.Where(game => game.CreatedAt >= targetPublishDate);
    }
}