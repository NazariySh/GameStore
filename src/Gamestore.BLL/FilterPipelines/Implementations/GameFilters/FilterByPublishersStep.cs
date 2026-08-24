using Gamestore.BLL.FilterPipelines.Interfaces;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Shared;

namespace Gamestore.BLL.FilterPipelines.Implementations.GameFilters;

public class FilterByPublishersStep : IFilterPipelineStep<Game>
{
    private readonly List<Guid> _publishers;
    private readonly List<int> _suppliers;

    public FilterByPublishersStep(ICollection<string> publishers)
    {
        var publisherIds = publishers.Select(EntityId.Parse).ToList();
        _publishers = publisherIds.Where(p => p.IsPrimary).Select(p => p.PrimaryId!.Value).ToList();
        _suppliers = publisherIds.Where(p => p.IsSecondary).Select(p => p.SecondaryId!.Value).ToList();
    }

    public IQueryable<Game> Apply(IQueryable<Game> query)
    {
        if (_publishers.Count == 0 && _suppliers.Count == 0)
        {
            return query;
        }

        return query.Where(game =>
            (_publishers.Count > 0 && _publishers.Contains(game.PublisherId)) ||
            (_suppliers.Count > 0 && game.SupplierId.HasValue && _suppliers.Contains(game.SupplierId.Value)));
    }
}