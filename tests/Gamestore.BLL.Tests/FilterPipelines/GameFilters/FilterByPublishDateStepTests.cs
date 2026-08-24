using Gamestore.BLL.Enums;
using Gamestore.BLL.FilterPipelines.Implementations.GameFilters;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Tests.FilterPipelines.GameFilters;

public class FilterByPublishDateStepTests
{
    [Fact]
    public void Apply_ShouldReturnAllGames_WhenPublishDateIsNull()
    {
        var games = GetGames();

        var step = new FilterByPublishDateStep(null);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Equivalent(games, result);
    }

    [Fact]
    public void Apply_ShouldReturnMatchingGames_WhenPublishDateIsValid()
    {
        var games = GetGames();

        var publishDateOption = PublishDateOption.LastMonth;

        var targetPublishDate = DateTime.UtcNow.Subtract(publishDateOption.PublishPeriod);

        var expectedGames = games
            .Where(game => game.CreatedAt >= targetPublishDate)
            .ToList();

        var step = new FilterByPublishDateStep(publishDateOption);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Equivalent(expectedGames, result);
    }

    private static List<Game> GetGames()
    {
        return GameTestData.GetGames();
    }
}