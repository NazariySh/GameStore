using Gamestore.DAL.Data.Initializers.Implementations.DataSeeders.DataExtractors;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;
using Microsoft.Extensions.Logging;

namespace Gamestore.DAL.Data.Initializers.Implementations.DataSeeders;

public class GameDataSeeder : BaseDataSeeder<Game, GameDataSeeder.GameModel>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Game> _gameRepository;

    public GameDataSeeder(
        IUnitOfWork unitOfWork,
        ISeedDataExtractor dataExtractor,
        ILogger<GameDataSeeder> logger)
        : base(unitOfWork, dataExtractor, logger)
    {
        _unitOfWork = unitOfWork;
        _gameRepository = unitOfWork.Repositories.Get<ISqlRepository<Game>>();
    }

    public override int Order => 2;

    protected override string FileName => "games";

    protected override async Task AddEntitiesAsync(ICollection<GameModel> models, CancellationToken cancellationToken)
    {
        var games = models.Select(MapToGame).ToList();

        await _gameRepository.AddRangeAsync(games, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static Game MapToGame(GameModel game)
    {
        return new Game
        {
            Id = game.Id,
            Name = game.Name,
            Key = game.Key,
            Description = game.Description,
            Price = game.Price,
            UnitInStock = game.UnitInStock,
            Discount = game.Discount,
            ViewCount = game.ViewCount,
            CommentCount = game.CommentCount,
            CreatedAt = game.CreatedAt,
            PublisherId = game.Publisher,
            ImageUrl = game.ImageUrl,
            GameGenres = game.Genres
                .Select(genreId => new GameGenre { GameId = game.Id, GenreId = genreId })
                .ToList(),
            GamePlatforms = game.Platforms
                .Select(platformId => new GamePlatform { GameId = game.Id, PlatformId = platformId })
                .ToList(),
        };
    }

    public sealed record GameModel(
        Guid Id,
        string Name,
        string Key,
        string? Description,
        decimal Price,
        int UnitInStock,
        int Discount,
        int ViewCount,
        int CommentCount,
        string? ImageUrl,
        DateTime CreatedAt,
        ICollection<Guid> Genres,
        ICollection<Guid> Platforms,
        Guid Publisher);
}