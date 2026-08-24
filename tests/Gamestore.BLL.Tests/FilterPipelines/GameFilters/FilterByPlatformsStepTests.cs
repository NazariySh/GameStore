using Gamestore.BLL.FilterPipelines.Implementations.GameFilters;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Tests.FilterPipelines.GameFilters;

public class FilterByPlatformsStepTests
{
    [Fact]
    public void Apply_ShouldReturnAllGames_WhenEmptyPlatforms()
    {
        var games = GetGames();
        var platforms = new List<Guid>();

        var step = new FilterByPlatformsStep(platforms);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Equivalent(games, result);
    }

    [Fact]
    public void Apply_ShouldReturnMatchingGames_WhenPlatformsMatch()
    {
        var games = GetGames();
        var platforms = games[0].GamePlatforms.Select(gp => gp.PlatformId).ToList();

        var expectedGames = games
            .Where(game => game.GamePlatforms.Any(gp => platforms.Contains(gp.PlatformId)))
            .ToList();

        var step = new FilterByPlatformsStep(platforms);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Equivalent(expectedGames, result);
    }

    private static List<Game> GetGames()
    {
        return GameTestData.GetGames();
    }
}