using Gamestore.BLL.FilterPipelines.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.FilterPipelines.Implementations.GameFilters;

public class FilterByGameNameStep : IFilterPipelineStep<Game>
{
    public const int MinNameLength = 3;

    private readonly string? _name;

    public FilterByGameNameStep(string? name)
    {
        _name = name?.Trim();
    }

    public IQueryable<Game> Apply(IQueryable<Game> query)
    {
        if (!IsValidName(_name))
        {
            return query;
        }

        var searchName = _name.ToLowerInvariant();
        return query.Where(game => game.Name.ToLower().Contains(searchName));
    }

    private static bool IsValidName(string? name)
    {
        return !string.IsNullOrEmpty(name) && name.Length >= MinNameLength;
    }
}