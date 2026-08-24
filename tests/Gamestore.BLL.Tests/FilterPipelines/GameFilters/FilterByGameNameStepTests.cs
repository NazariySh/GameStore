using Gamestore.BLL.FilterPipelines.Implementations.GameFilters;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Tests.FilterPipelines.GameFilters;

public class FilterByGameNameStepTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Apply_ShouldReturnAllGames_WhenNameIsEmpty(string invalidName)
    {
        var games = GetGames();

        var step = new FilterByGameNameStep(invalidName);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Equivalent(games, result);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("  a  ")]
    [InlineData("  ab  ")]
    public void Apply_ShouldReturnAllGames_WhenNameIsShorterThanMinLength(string shortName)
    {
        var games = GetGames();

        var step = new FilterByGameNameStep(shortName);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Equivalent(games, result);
    }

    [Fact]
    public void Apply_ShouldReturnMatchingGames_WhenNameMatchesExactly()
    {
        var games = GetGames();
        var gameName = games[0].Name;

        var step = new FilterByGameNameStep(gameName);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Single(result);
        Assert.Equivalent(games[0], result[0]);
    }

    [Fact]
    public void Apply_ShouldReturnMatchingGames_WhenNameMatchesPartially()
    {
        var games = GetGames();
        var partialName = games[0].Name[..3];

        var expectedGames = games
            .Where(g => g.Name.Contains(partialName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var step = new FilterByGameNameStep(partialName);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Equivalent(expectedGames, result);
    }

    [Fact]
    public void Apply_ShouldBeCaseInsensitive_WhenSearchingByName()
    {
        var games = GetGames();
        var gameName = games[0].Name.ToUpperInvariant();

        var step = new FilterByGameNameStep(gameName);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Single(result);
        Assert.Equivalent(games[0], result[0]);
    }

    private static List<Game> GetGames()
    {
        return GameTestData.GetGames();
    }
}