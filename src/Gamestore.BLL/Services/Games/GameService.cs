using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Gamestore.BLL.DTOs;
using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.DTOs.Games.Genres;
using Gamestore.BLL.DTOs.Games.Platforms;
using Gamestore.BLL.DTOs.Games.Publishers;
using Gamestore.BLL.DTOs.Logging;
using Gamestore.BLL.Enums;
using Gamestore.BLL.FilterPipelines.Implementations;
using Gamestore.BLL.FilterPipelines.Implementations.GameFilters;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Games;
using Gamestore.BLL.Interfaces.Logging;
using Gamestore.BLL.Utilities;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Repositories.Interfaces.Games;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Constants;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Enums;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Shared;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#pragma warning disable S6966

namespace Gamestore.BLL.Services.Games;

public class GameService : IGameService
{
    public const string TotalGameCountCacheKey = "TotalGameCount";
    public const string GameImageCacheKeyPrefix = "game-image:";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IGameRepository _gameRepository;
    private readonly IRepository<Genre> _genreRepository;
    private readonly IRepository<Platform> _platformRepository;
    private readonly IRepository<Publisher> _publisherRepository;
    private readonly IMapper _mapper;
    private readonly IValidationService _validationService;
    private readonly ICacheService _cacheService;
    private readonly IGameKeyGenerator _gameKeyGenerator;
    private readonly IGameImageService _gameImageService;
    private readonly IEntityChangeLogService _entityChangeLogService;
    private readonly IUserContext _userContext;
    private readonly ILogger<GameService> _logger;

    public GameService(
        IServiceContext context,
        ICacheService cacheService,
        IGameKeyGenerator gameKeyGenerator,
        IGameImageService gameImageService,
        IEntityChangeLogService entityChangeLogService,
        ILogger<GameService> logger)
    {
        _unitOfWork = context.UnitOfWork;
        _gameRepository = _unitOfWork.Repositories.Get<IGameRepository>();
        _genreRepository = _unitOfWork.Repositories.GetGeneric<Genre>();
        _platformRepository = _unitOfWork.Repositories.GetGeneric<Platform>();
        _publisherRepository = _unitOfWork.Repositories.GetGeneric<Publisher>();
        _mapper = context.Mapper;
        _validationService = context.Validation;
        _cacheService = cacheService;
        _gameKeyGenerator = gameKeyGenerator;
        _gameImageService = gameImageService;
        _entityChangeLogService = entityChangeLogService;
        _userContext = context.UserContext;
        _logger = logger;
    }

    public async Task<PagedGameList<GameDto>> GetAllFilteredAsync(GameQueryDto query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve all games with query: {@Query}", query);

        var page = Math.Max(query.Page, PaginationOption.DefaultPage);
        var pageSize = Math.Max(query.PageSize, PaginationOption.DefaultPageSize);

        var baseFilterPipeline = new QueryPipelineBuilder<Game>()
            .AddFilterStep(new FilterStep<Game>(g => g.UnitInStock > 0))
            .AddFilterStep(new FilterByGenresStep(query.Genres))
            .AddFilterStep(new FilterByPlatformsStep(query.Platforms))
            .AddFilterStep(new FilterByPublishersStep(query.Publishers))
            .AddFilterStep(new FilterByPriceRangeStep(query.MinPrice, query.MaxPrice))
            .AddFilterStep(new FilterByPublishDateStep(PublishDateOption.FromDisplayNameOrDefault(query.DatePublishing)))
            .AddFilterStep(new FilterByGameNameStep(query.Name));

        var totalCount = await _gameRepository.CountAsync(baseFilterPipeline.Build(), cancellationToken);

        pageSize = Math.Max(Math.Min(pageSize, totalCount), PaginationOption.DefaultPageSize);

        var baseSortPipeline = new QueryPipelineBuilder<Game>()
            .AddSortStep(new GameSortStep(GameSortOption.FromDisplayNameOrDefault(query.Sort)));

        var queryPipeline = baseFilterPipeline.Combine(baseSortPipeline).Build();

        var paginationPipeline = baseSortPipeline
            .SetPaginationStep(new PaginationStep<Game>(page, pageSize))
            .Build();

        var games = await _gameRepository.GetAllAsync(queryPipeline, cancellationToken: cancellationToken);

        games = paginationPipeline(games.AsQueryable()).ToList();

        var gameDtos = _mapper.Map<IReadOnlyList<GameDto>>(games);

        _logger.LogInformation("Retrieved {Count} games from database with query: {@Query}", gameDtos.Count, query);

        return new PagedGameList<GameDto>(gameDtos, page, pageSize, totalCount);
    }

    public async Task<IReadOnlyList<GameDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve all games");

        var games = await _gameRepository.GetAllAsync<GameDto>(cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} games from database", games.Count);

        return games;
    }

    public async Task<IReadOnlyList<GenreDto>> GetGameGenresAsync(string gameKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve all genres for game with key '{GameKey}'", gameKey);

        Guard.AgainstNullOrWhiteSpace(gameKey);

        var game = await GetGameByKeyOrThrowAsync(
            gameKey,
            g => new Game { GameGenres = g.GameGenres, CategoryId = g.CategoryId },
            cancellationToken);

        var genreIds = game.GameGenres.Select(gg => gg.GenreId).ToList();

        var genres = await _genreRepository.GetAllAsync<GenreDto>(
            g =>
                genreIds.Contains(g.Id) ||
                (game.CategoryId.HasValue && g.CategoryId == game.CategoryId),
            cancellationToken);

        _logger.LogInformation("Retrieved {Count} genres for game with key '{GameKey}' from database", genres.Count, gameKey);

        return genres;
    }

    public async Task<IReadOnlyList<PlatformDto>> GetGamePlatformsAsync(string gameKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve platforms for game with key '{GameKey}'", gameKey);

        Guard.AgainstNullOrWhiteSpace(gameKey);

        var game = await GetGameByKeyOrThrowAsync(
            gameKey,
            g => new Game { GamePlatforms = g.GamePlatforms },
            cancellationToken);

        var platformIds = game.GamePlatforms.Select(gp => gp.PlatformId).ToList();

        var platforms = await _platformRepository.GetAllAsync<PlatformDto>(
            p => platformIds.Contains(p.Id),
            cancellationToken);

        _logger.LogInformation("Retrieved {Count} platforms for game with key '{GameKey}' from database", platforms.Count, gameKey);

        return platforms;
    }

    public async Task<PublisherDto> GetGamePublisherAsync(string gameKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve publisher for game with key '{GameKey}'", gameKey);

        Guard.AgainstNullOrWhiteSpace(gameKey);

        var game = await GetGameByKeyOrThrowAsync(
            gameKey,
            g => new Game { PublisherId = g.PublisherId, SupplierId = g.SupplierId },
            cancellationToken);

        var publisher = await _publisherRepository.GetSingleAsync<PublisherDto>(
            p =>
                (game.PublisherId != Guid.Empty && p.Id == game.PublisherId) ||
                (game.SupplierId.HasValue && p.SupplierId == game.SupplierId),
            cancellationToken);

        if (publisher is null)
        {
            _logger.LogError("Publisher for game with key '{GameKey}' not found", gameKey);
            throw new NotFoundException($"Publisher for game with key '{gameKey}' not found.");
        }

        _logger.LogInformation("Retrieved publisher for game with key '{GameKey}' from database", gameKey);

        return publisher;
    }

    public async Task<DownloadGameImageDto> GetGameImageAsync(string gameKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve image for game with key '{GameKey}'", gameKey);

        Guard.AgainstNullOrWhiteSpace(gameKey);

        var cachedImage = await _cacheService.GetOrCreateAsync(
            $"{GameImageCacheKeyPrefix}{gameKey}",
            async token =>
            {
                _logger.LogInformation("Cache miss for game image with key '{GameKey}' - fetching from storage", gameKey);

                var game = await GetGameByKeyOrThrowAsync(
                    gameKey,
                    g => new Game { ImageUrl = g.ImageUrl },
                    token);

                if (string.IsNullOrEmpty(game.ImageUrl))
                {
                    _logger.LogWarning("Game with key '{GameKey}' does not have an image", gameKey);
                    return new DownloadGameImageDto
                    {
                        FileName = "empty-image",
                        ContentType = "image/png",
                        Content = [],
                    };
                }

                return await _gameImageService.DownloadByUrlAsync(game.ImageUrl, token);
            },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved image for game with key '{GameKey}'", gameKey);

        return cachedImage;
    }

    public async Task<GameDto> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve game with Id '{Id}'", id);

        var gameId = EntityId.Parse(id);

        Guard.AgainstEmpty(gameId);

        var game = await GetGameOrThrowAsync(gameId, cancellationToken);

        var gameDto = _mapper.Map<GameDto>(game);

        game.ViewCount++;

        await _gameRepository.UpdateViewCountAsync(game, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Retrieved game with Id '{Id}' from database", id);

        return gameDto;
    }

    public async Task<GameDto> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve game with key '{Key}'", key);

        Guard.AgainstNullOrWhiteSpace(key);

        var game = await GetGameByKeyOrThrowAsync(key, cancellationToken);

        var gameDto = _mapper.Map<GameDto>(game);

        game.ViewCount++;

        await _gameRepository.UpdateViewCountAsync(game, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Retrieved game with key '{Key}' from database", key);

        return gameDto;
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve total game count");

        var count = await _cacheService.GetOrCreateAsync(
            TotalGameCountCacheKey,
            async token =>
            {
                _logger.LogInformation("Cache miss for total game count - fetching from database");

                return await _gameRepository.CountAsync(token);
            },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved total game count: {Count}", count);

        return count;
    }

    public async Task<DownloadFileContentDto> DownloadFileAsync(string gameKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to download file for game with key '{GameKey}'", gameKey);

        Guard.AgainstNullOrWhiteSpace(gameKey);

        var game = await GetGameByKeyWithDetailsOrThrowAsync(gameKey, cancellationToken);

        var gameDto = _mapper.Map<GameDetailedDto>(game);

        var fileContent = FileContentExporter.ExportToTextFile(gameDto, GetFileName(gameDto.Name));

        _logger.LogInformation("File {FileName} content created successfully for game with key '{GameKey}'", fileContent.FileName, gameKey);

        return fileContent;
    }

    public async Task<CreateGameResponse> CreateAsync(CreateGameRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to create a new game with name '{Name}'", request.Game.Name);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var newGame = _mapper.Map<Game>(request);
        newGame.CreatedAt = DateTime.UtcNow;

        if (string.IsNullOrEmpty(newGame.Key))
        {
            newGame.Key = await _gameKeyGenerator.GenerateUniqueAsync(newGame.Name, cancellationToken);
        }

        UpdateGameGenres(newGame, request.Genres);
        UpdateGamePlatforms(newGame, request.Platforms);

        if (!string.IsNullOrWhiteSpace(request.Image))
        {
            await UpdateGameImageAsync(newGame, request.Image, cancellationToken);
        }

        await _gameRepository.AddAsync(newGame, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Game>(LogAction.Add, newVersion: newGame), cancellationToken);

        await _cacheService.RemoveAsync(TotalGameCountCacheKey, cancellationToken);
        await _cacheService.RemoveAsync($"{GameImageCacheKeyPrefix}{newGame.Key}", cancellationToken);

        _logger.LogInformation("Game with Id '{Id}' created successfully", newGame.Id);

        return new CreateGameResponse
        {
            Id = newGame.Id,
            Key = newGame.Key,
        };
    }

    public async Task UpdateAsync(UpdateGameRequest request, CancellationToken cancellationToken = default)
    {
        var game = request.Game;

        _logger.LogInformation("Attempting to update game with Id '{Id}'", game.Id);

        Guard.AgainstEmpty(game.Id);

        await EnsureGameExistsAsync(game.Id, cancellationToken);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var existingGame = await GetGameWithDetailsOrThrowAsync(game.Id, cancellationToken);

        EnsureCanEditGameIfDeleted(existingGame);

        var oldVersion = _mapper.Map<Game>(existingGame);

        _mapper.Map(request, existingGame);

        UpdateGameGenres(existingGame, request.Genres);
        UpdateGamePlatforms(existingGame, request.Platforms);

        if (!string.IsNullOrWhiteSpace(request.Image))
        {
            await UpdateGameImageAsync(existingGame, request.Image, cancellationToken);
        }

        await _gameRepository.UpdateAsync(existingGame, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Game>(LogAction.Update, oldVersion, existingGame), cancellationToken);

        await _cacheService.RemoveAsync(TotalGameCountCacheKey, cancellationToken);
        await _cacheService.RemoveAsync($"{GameImageCacheKeyPrefix}{existingGame.Key}", cancellationToken);

        _logger.LogInformation("Game with Id '{Id}' updated successfully", game.Id);
    }

    public async Task DeleteByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete game with key '{Key}'", key);

        Guard.AgainstNullOrWhiteSpace(key);

        var game = await GetGameByKeyOrThrowAsync(key, cancellationToken);

        EnsureGameNotDeleted(game);

        var oldVersion = _mapper.Map<Game>(game);

        await _gameImageService.DeleteAsync(game.Key, cancellationToken);

        await _gameRepository.SoftDeleteAsync(game, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Game>(LogAction.SoftDelete, oldVersion, game), cancellationToken);

        await _cacheService.RemoveAsync(TotalGameCountCacheKey, cancellationToken);
        await _cacheService.RemoveAsync($"{GameImageCacheKeyPrefix}{key}", cancellationToken);

        _logger.LogInformation("Game with key '{Key}' deleted successfully", key);
    }

    private static void UpdateGameGenres(Game game, ICollection<EntityId> genreIds)
    {
        var genres = genreIds.Where(g => g.IsPrimary).Select(g => g.PrimaryId!.Value).ToList();
        var categories = genreIds.Where(g => g.IsSecondary).Select(g => g.SecondaryId!.Value).ToList();

        var existingGenreIds = game.GameGenres.Select(gg => gg.GenreId).ToList();
        if (existingGenreIds.SequenceEqual(genres) && categories.Count == 0)
        {
            return;
        }

        var gameGenres = genres
            .Select(genreId => new GameGenre { GameId = game.Id, GenreId = genreId });

        var gameCategories = categories
            .Select(categoryId => new GameGenre { GameId = game.Id, CategoryId = categoryId });

        game.GameGenres = gameGenres.Concat(gameCategories).ToList();
    }

    private static void UpdateGamePlatforms(Game game, ICollection<Guid> platformIds)
    {
        var existingPlatformIds = game.GamePlatforms.Select(gp => gp.PlatformId).ToList();
        if (existingPlatformIds.SequenceEqual(platformIds))
        {
            return;
        }

        game.GamePlatforms = platformIds
            .Select(platformId => new GamePlatform { GameId = game.Id, PlatformId = platformId })
            .ToList();
    }

    private async Task UpdateGameImageAsync(Game game, string base64Image, CancellationToken cancellationToken)
    {
        var (imageBytes, contentType) = Base64ImageParser.Parse(base64Image);

        var imageUrl = await _gameImageService.UploadAsync(game.Key, imageBytes, contentType, cancellationToken);
        game.ImageUrl = imageUrl;

        _logger.LogInformation("Image uploaded successfully for game with key '{Key}'", game.Key);
    }

    private async Task<Game> GetGameWithDetailsOrThrowAsync(EntityId id, CancellationToken cancellationToken)
    {
        Game? game;

        if (id.IsPrimary)
        {
            game = await _gameRepository.GetSingleAsync(
                g => g.Id == id.PrimaryId,
                include: x => x
                    .Include(g => g.GameGenres)
                    .Include(g => g.GamePlatforms),
                cancellationToken);
        }
        else
        {
            game = await _gameRepository.GetAsync(
                g => g.ProductId == id.SecondaryId,
                cancellationToken: cancellationToken);
        }

        EnsureGameExists(game, id);

        return game;
    }

    private async Task<Game> GetGameByKeyWithDetailsOrThrowAsync(string key, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetSingleAsync(
            g => g.Key == key,
            x => x
                .Include(g => g.GameGenres)
                .Include(g => g.GamePlatforms),
            cancellationToken: cancellationToken);

        EnsureGameWithKeyExists(game, key);

        return game;
    }

    private async Task<Game> GetGameByKeyOrThrowAsync(
        string key,
        Expression<Func<Game, Game>> selector,
        CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetFirstSelectedAsync(
            g => g.Key == key,
            selector,
            cancellationToken);

        EnsureGameWithKeyExists(game, key);

        return game;
    }

    private async Task<Game> GetGameByKeyOrThrowAsync(string key, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetSingleAsync(
            g => g.Key == key,
            cancellationToken: cancellationToken);

        EnsureGameWithKeyExists(game, key);

        return game;
    }

    private async Task<Game> GetGameOrThrowAsync(EntityId id, CancellationToken cancellationToken)
    {
        Game? game;

        if (id.IsPrimary)
        {
            game = await _gameRepository.GetSingleAsync(
                g => g.Id == id.PrimaryId,
                cancellationToken: cancellationToken);
        }
        else
        {
            game = await _gameRepository.GetAsync(
                g => g.ProductId == id.SecondaryId,
                cancellationToken: cancellationToken);
        }

        EnsureGameExists(game, id);

        return game;
    }

    private void EnsureGameWithKeyExists<T>([NotNull] T? game, string key)
    {
        if (game is null)
        {
            _logger.LogError("Game with key '{Key}' not found", key);
            throw new GameNotFoundException(key);
        }
    }

    private void EnsureGameExists<T>([NotNull] T? game, EntityId id)
    {
        if (game is null)
        {
            ThrowGameNotFound(id);
        }
    }

    private void EnsureCanEditGameIfDeleted(Game game)
    {
        if (game.IsDeleted && !_userContext.HasPermission(Permissions.EditDeletedGames))
        {
            _logger.LogError("User does not have permission to edit deleted games");
            throw new ForbiddenException("You do not have permission to edit deleted games.");
        }
    }

    private void EnsureGameNotDeleted(Game game)
    {
        if (game.IsDeleted)
        {
            _logger.LogError("Game with key '{Key}' is already deleted", game.Key);
            throw new ConflictException($"Game with key '{game.Key}' is already deleted.");
        }
    }

    private async Task EnsureGameExistsAsync(EntityId id, CancellationToken cancellationToken)
    {
        if (!await GameExistsAsync(id, cancellationToken))
        {
            ThrowGameNotFound(id);
        }
    }

    private Task<bool> GameExistsAsync(EntityId id, CancellationToken cancellationToken)
    {
        return id.IsPrimary
            ? _gameRepository.ExistsAsync(g => g.Id == id.PrimaryId, cancellationToken)
            : _gameRepository.ExistsAsync(g => g.ProductId == id.SecondaryId, cancellationToken);
    }

    private static string GetFileName(string name)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"{name}_{timestamp}";
    }

    [DoesNotReturn]
    private void ThrowGameNotFound(EntityId id)
    {
        _logger.LogError("Game with Id '{Id}' not found", id.ToString());
        throw new GameNotFoundException(id);
    }
}