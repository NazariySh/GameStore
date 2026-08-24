using Gamestore.BLL.FilterPipelines.Interfaces;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Shared;

namespace Gamestore.BLL.FilterPipelines.Implementations.GameFilters;

public class FilterByGenresStep : IFilterPipelineStep<Game>
{
    private readonly List<Guid> _genres;
    private readonly List<int> _categories;

    public FilterByGenresStep(ICollection<string> genres)
    {
        var genreIds = genres.Select(EntityId.Parse).ToList();
        _genres = genreIds.Where(g => g.IsPrimary).Select(g => g.PrimaryId!.Value).ToList();
        _categories = genreIds.Where(g => g.IsSecondary).Select(g => g.SecondaryId!.Value).ToList();
    }

    public IQueryable<Game> Apply(IQueryable<Game> query)
    {
        if (_genres.Count == 0 && _categories.Count == 0)
        {
            return query;
        }

        return query.Where(game =>
            (_genres.Count > 0 && game.GameGenres.Any(gg => _genres.Contains(gg.GenreId))) ||
            (_categories.Count > 0 && game.CategoryId.HasValue && _categories.Contains(game.CategoryId.Value)));
    }
}