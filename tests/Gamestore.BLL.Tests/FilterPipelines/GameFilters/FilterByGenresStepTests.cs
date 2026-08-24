using Gamestore.BLL.FilterPipelines.Implementations.GameFilters;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Tests.FilterPipelines.GameFilters;

public class FilterByGenresStepTests
{
    [Fact]
    public void Apply_ShouldReturnAllGames_WhenEmptyGenres()
    {
        var games = GetGames();
        var genres = new List<string>();

        var step = new FilterByGenresStep(genres);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Equivalent(games, result);
    }

    [Fact]
    public void Apply_ShouldReturnMatchingGames_WhenGenresMatch()
    {
        var games = GetGames();
        var genres = games[0].GameGenres.Select(gg => gg.GenreId).ToList();

        var expectedGames = games
            .Where(game => game.GameGenres.Any(gg => genres.Contains(gg.GenreId)))
            .ToList();

        var step = new FilterByGenresStep(genres.Select(g => g.ToString()).ToList());
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Equivalent(expectedGames, result);
    }

    private static List<Game> GetGames()
    {
        return GameTestData.GetGames();
    }
}