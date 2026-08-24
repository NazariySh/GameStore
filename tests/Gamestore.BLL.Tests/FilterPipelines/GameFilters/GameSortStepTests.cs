using Gamestore.BLL.Enums;
using Gamestore.BLL.FilterPipelines.Implementations.GameFilters;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Tests.FilterPipelines.GameFilters;

public class GameSortStepTests
{
    [Fact]
    public void Apply_ShouldReturnAllGames_WhenSortByIsNull()
    {
        var games = GetGames();

        var step = new GameSortStep(null);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.Equivalent(games, result);
    }

    [Theory]
    [MemberData(nameof(ValidSortResults))]
    public void Apply_ShouldSortByOption_WhenSortByIsValid(GameSortOption option, Game firstResultGame)
    {
        var games = GetGames();

        var step = new GameSortStep(option);
        var result = step.Apply(games.AsQueryable()).ToList();

        Assert.NotEmpty(result);
        Assert.Equivalent(firstResultGame, result[0]);
    }

    public static TheoryData<GameSortOption, Game> ValidSortResults()
    {
        return new TheoryData<GameSortOption, Game>
        {
            { GameSortOption.MostPopular, GetMostPopularGame() },
            { GameSortOption.MostCommented, GetMostCommentedGame() },
            { GameSortOption.PriceAsc, GetGameWithLowestPrice() },
            { GameSortOption.PriceDesc, GetGameWithHighestPrice() },
            { GameSortOption.New, GetNewGame() },
        };
    }

    private static List<Game> GetGames()
    {
        return
        [
            GetMostPopularGame(),
            GetMostCommentedGame(),
            GetGameWithHighestPrice(),
            GetGameWithLowestPrice(),
            GetNewGame()
        ];
    }

    private static Game GetMostPopularGame()
    {
        return new Game
        {
            Id = Guid.NewGuid(),
            Name = "Most Popular Game",
            ViewCount = 1000,
            CommentCount = 50,
            Price = 59.99m,
            CreatedAt = DateTime.UtcNow.AddDays(-30).Date,
        };
    }

    private static Game GetMostCommentedGame()
    {
        return new Game
        {
            Id = Guid.NewGuid(),
            Name = "Most Commented Game",
            ViewCount = 500,
            CommentCount = 200,
            Price = 49.99m,
            CreatedAt = DateTime.UtcNow.AddDays(-60).Date,
        };
    }

    private static Game GetGameWithHighestPrice()
    {
        return new Game
        {
            Id = Guid.NewGuid(),
            Name = "High Price Game",
            ViewCount = 300,
            CommentCount = 30,
            Price = 99.99m,
            CreatedAt = DateTime.UtcNow.AddDays(-90).Date,
        };
    }

    private static Game GetGameWithLowestPrice()
    {
        return new Game
        {
            Id = Guid.NewGuid(),
            Name = "Low Price Game",
            ViewCount = 200,
            CommentCount = 20,
            Price = 19.99m,
            CreatedAt = DateTime.UtcNow.AddDays(-120).Date,
        };
    }

    private static Game GetNewGame()
    {
        return new Game
        {
            Id = Guid.NewGuid(),
            Name = "New Game",
            ViewCount = 150,
            CommentCount = 10,
            Price = 39.99m,
            CreatedAt = DateTime.UtcNow.AddDays(-1).Date,
        };
    }
}