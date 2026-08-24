using Gamestore.BLL.FilterPipelines.Implementations.GameFilters;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Tests.FilterPipelines.GameFilters;

public class FilterByPriceRangeStepTests
{
    [Fact]
    public void Apply_ShouldReturnAllGames_WhenNoPriceRange()
    {
        var games = GetGames();

        var step = new FilterByPriceRangeStep(null, null);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Equivalent(games, result);
    }

    [Fact]
    public void Apply_ShouldReturnMatchingGames_WhenOnlyMinPriceIsSet()
    {
        const decimal minPrice = 20m;
        var games = GetGames();

        var expectedGames = games
            .Where(game => game.Price >= minPrice)
            .ToList();

        var step = new FilterByPriceRangeStep(minPrice, null);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Equivalent(expectedGames, result);
    }

    [Fact]
    public void Apply_ShouldReturnMatchingGames_WhenOnlyMaxPriceIsSet()
    {
        const decimal maxPrice = 30m;
        var games = GetGames();

        var expectedGames = games
            .Where(game => game.Price <= maxPrice)
            .ToList();

        var step = new FilterByPriceRangeStep(null, maxPrice);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Equivalent(expectedGames, result);
    }

    [Fact]
    public void Apply_ShouldReturnMatchingGames_WhenPriceRangeMatches()
    {
        const decimal minPrice = 10m;
        const decimal maxPrice = 50m;
        var games = GetGames();

        var expectedGames = games
            .Where(game => game.Price is >= minPrice and <= maxPrice)
            .ToList();

        var step = new FilterByPriceRangeStep(minPrice, maxPrice);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Equivalent(expectedGames, result);
    }

    private static List<Game> GetGames()
    {
        return GameTestData.GetGames();
    }
}