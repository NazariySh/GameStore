using Gamestore.BLL.FilterPipelines.Implementations.GameFilters;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Tests.FilterPipelines.GameFilters;

public class FilterByPublishersStepTests
{
    [Fact]
    public void Apply_ShouldReturnAllGames_WhenEmptyPublishers()
    {
        var games = GetGames();
        var publishers = new List<string>();

        var step = new FilterByPublishersStep(publishers);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Equivalent(games, result);
    }

    [Fact]
    public void Apply_ShouldReturnMatchingGames_WhenPublishersMatch()
    {
        var games = GetGames();

        var publishers = new List<Guid>
        {
            games[0].PublisherId,
            games[1].PublisherId,
        };

        var expectedGames = games
            .Where(game => publishers.Contains(game.PublisherId))
            .ToList();

        var step = new FilterByPublishersStep(publishers.Select(p => p.ToString()).ToList());
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Equivalent(expectedGames, result);
    }

    private static List<Game> GetGames()
    {
        return GameTestData.GetGames();
    }
}