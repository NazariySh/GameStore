using FluentValidation;
using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.DTOs.Games.Genres;
using Gamestore.BLL.DTOs.Games.Platforms;
using Gamestore.BLL.DTOs.Games.Publishers;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Games;
using Gamestore.BLL.Interfaces.Logging;
using Gamestore.BLL.Services;
using Gamestore.BLL.Services.Games;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Constants;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Exceptions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Services.Games;

public class GameServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Game> _gameRepository;
    private readonly IRepository<Genre> _genreRepository;
    private readonly IRepository<Platform> _platformRepository;
    private readonly IRepository<Publisher> _publisherRepository;
    private readonly IMapper _mapper;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IGameKeyGenerator> _mockGameKeyGenerator;
    private readonly Mock<IGameImageService> _mockGameImageService;
    private readonly GameService _gameService;

    public GameServiceTests()
    {
        _mockUserContext = new Mock<IUserContext>();
        _mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, false);
        _mockUserContext.SetupHasPermission(Permissions.EditDeletedGames, false);

        _unitOfWork = UnitOfWorkFactory.Create(_mockUserContext.Object);
        _gameRepository = _unitOfWork.Repositories.GetGeneric<Game>();
        _genreRepository = _unitOfWork.Repositories.GetGeneric<Genre>();
        _platformRepository = _unitOfWork.Repositories.GetGeneric<Platform>();
        _publisherRepository = _unitOfWork.Repositories.GetGeneric<Publisher>();

        _mapper = MapperFactory.Create();
        _mockValidationService = new Mock<IValidationService>();
        _mockCacheService = new Mock<ICacheService>();
        _mockGameKeyGenerator = new Mock<IGameKeyGenerator>();
        _mockGameImageService = new Mock<IGameImageService>();

        var mockEntityChangeLogService = new Mock<IEntityChangeLogService>();

        _gameService = new GameService(
            new ServiceContext(_unitOfWork, _mapper, _mockValidationService.Object, _mockUserContext.Object),
            _mockCacheService.Object,
            _mockGameKeyGenerator.Object,
            _mockGameImageService.Object,
            mockEntityChangeLogService.Object,
            Mock.Of<ILogger<GameService>>());
    }

    [Fact]
    public async Task GetAllFilteredAsync_ShouldReturnEmptyList_WhenNoGamesMatchFilter()
    {
        await SeedGamesAsync();

        var query = new GameQueryDto
        {
            Name = "NonExistentGame",
            Page = 1,
            PageSize = 3,
        };

        var result = await _gameService.GetAllFilteredAsync(query);

        Assert.Empty(result.Games);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task GetAllFilteredAsync_ShouldReturnDeletedGames_WhenUserCanViewDeletedGames()
    {
        var games = await SeedGamesAsync();
        var query = new GameQueryDto
        {
            Page = 1,
            PageSize = 10,
        };

        var totalPages = (int)Math.Ceiling(games.Count / (double)query.PageSize);
        var expectedGames = games
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var expectedGameDtos = _mapper.Map<List<GameDto>>(expectedGames);

        _mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, true);

        var result = await _gameService.GetAllFilteredAsync(query);

        Assert.NotEmpty(result.Games);
        Assert.Equivalent(expectedGameDtos, result.Games);
        Assert.Equal(query.Page, result.CurrentPage);
        Assert.Equal(totalPages, result.TotalPages);
    }

    [Fact]
    public async Task GetAllFilteredAsync_ShouldApplyFilter_WhenFilterIsValid()
    {
        var games = await SeedGamesAsync();

        var query = new GameQueryDto
        {
            Name = "Fan",
            Page = 2,
            PageSize = 1,
        };

        var filteredGames = games
            .Where(g => !g.IsDeleted)
            .Where(g => g.Name.ToLower().Contains(query.Name.ToLower()))
            .ToList();

        var totalPages = (int)Math.Ceiling(filteredGames.Count / (double)query.PageSize);

        var expectedGames = filteredGames
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var expectedGameDtos = _mapper.Map<List<GameDto>>(expectedGames);

        var result = await _gameService.GetAllFilteredAsync(query);

        Assert.NotEmpty(result.Games);
        Assert.Equivalent(expectedGameDtos, result.Games);
        Assert.Equal(query.Page, result.CurrentPage);
        Assert.Equal(totalPages, result.TotalPages);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnGames_WhenGamesExist()
    {
        var games = await SeedGamesAsync();

        games = games.Where(g => !g.IsDeleted).ToList();

        var gameDtos = _mapper.Map<List<GameDto>>(games);

        var result = await _gameService.GetAllAsync();

        Assert.NotEmpty(result);
        Assert.Equivalent(gameDtos, result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnDeletedGames_WhenUserCanViewDeletedGames()
    {
        var games = await SeedGamesAsync();

        var gameDtos = _mapper.Map<List<GameDto>>(games);

        _mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, true);

        var result = await _gameService.GetAllAsync();

        Assert.NotEmpty(result);
        Assert.Equivalent(gameDtos, result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoGamesExist()
    {
        var result = await _gameService.GetAllAsync();

        Assert.Empty(result);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task GetGameGenresAsync_ShouldThrowArgumentException_WhenGameKeyIsInvalid(string invalidGameKey)
    {
        var act = () => _gameService.GetGameGenresAsync(invalidGameKey);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetGameGenresAsync_ShouldThrowGameNotFoundException_WhenGameDoesNotExist()
    {
        const string gameKey = "not-found";

        var act = () => _gameService.GetGameGenresAsync(gameKey);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task GetGameGenresAsync_ShouldThrowGameNotFoundException_WhenGameIsDeletedAndCannotViewDeletedGames()
    {
        var game = await SeedDeletedGameAsync();

        var act = () => _gameService.GetGameGenresAsync(game.Key);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task GetGameGenresAsync_ShouldReturnGenres_WhenGameKeyIsValid()
    {
        var (game, genres) = await SeedGameWithGenresAsync();

        var genreDtos = _mapper.Map<List<GenreDto>>(genres);

        var result = await _gameService.GetGameGenresAsync(game.Key);

        Assert.NotEmpty(result);
        Assert.Equivalent(genreDtos, result);
    }

    [Fact]
    public async Task GetGameGenresAsync_ShouldReturnEmptyList_WhenNoGenresExistForGame()
    {
        var game = await SeedGameAsync();

        var result = await _gameService.GetGameGenresAsync(game.Key);

        Assert.Empty(result);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task GetGamePlatformsAsync_ShouldThrowArgumentException_WhenGameKeyIsInvalid(string invalidGameKey)
    {
        var act = () => _gameService.GetGamePlatformsAsync(invalidGameKey);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetGamePlatformsAsync_ShouldThrowGameNotFoundException_WhenGameDoesNotExist()
    {
        const string gameKey = "not-found";

        var act = () => _gameService.GetGamePlatformsAsync(gameKey);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task GetGamePlatformsAsync_ShouldThrowGameNotFoundException_WhenGameIsDeletedAndUserCannotViewDeletedGames()
    {
        var game = await SeedDeletedGameAsync();

        var act = () => _gameService.GetGamePlatformsAsync(game.Key);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task GetGamePlatformsAsync_ShouldReturnPlatforms_WhenGameKeyIsValid()
    {
        var (game, platforms) = await SeedGameWithPlatformsAsync();

        var platformDtos = _mapper.Map<List<PlatformDto>>(platforms);

        var result = await _gameService.GetGamePlatformsAsync(game.Key);

        Assert.NotEmpty(result);
        Assert.Equivalent(platformDtos, result);
    }

    [Fact]
    public async Task GetGamePlatformsAsync_ShouldReturnEmptyList_WhenNoPlatformsExistForGame()
    {
        var game = await SeedGameAsync();

        var result = await _gameService.GetGamePlatformsAsync(game.Key);

        Assert.Empty(result);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task GetGamePublisherAsync_ShouldThrowArgumentException_WhenGameKeyIsInvalid(string invalidGameKey)
    {
        var act = () => _gameService.GetGamePublisherAsync(invalidGameKey);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetGamePublisherAsync_ShouldThrowGameNotFoundException_WhenGameDoesNotExist()
    {
        const string gameKey = "not-found";

        var act = () => _gameService.GetGamePublisherAsync(gameKey);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task GetGamePublisherAsync_ShouldThrowGameNotFoundException_WhenGameIsDeletedAndUserCannotViewDeletedGames()
    {
        var game = await SeedDeletedGameAsync();

        var act = () => _gameService.GetGamePublisherAsync(game.Key);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task GetGamePublisherAsync_ShouldThrowNotFoundException_WhenPublisherDoesNotExist()
    {
        var game = await SeedGameAsync();

        var act = () => _gameService.GetGamePublisherAsync(game.Key);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task GetGamePublisherAsync_ShouldReturnPublisher_WhenGameKeyIsValid()
    {
        var (game, publisher) = await SeedGameWithPublisherAsync();

        var publisherDto = _mapper.Map<PublisherDto>(publisher);

        var result = await _gameService.GetGamePublisherAsync(game.Key);

        Assert.NotNull(result);
        Assert.Equivalent(publisherDto, result);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task GetGameImageAsync_ShouldThrowArgumentException_WhenGameKeyIsInvalid(string invalidGameKey)
    {
        var act = () => _gameService.GetGameImageAsync(invalidGameKey);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetGameImageAsync_ShouldReturnDownloadImageContentFromCache_WhenImageExistInCache()
    {
        var game = await SeedGameAsync();

        var downloadContent = new DownloadGameImageDto
        {
            FileName = game.Key,
            ContentType = "image/png",
            Content = [1, 2, 3],
        };

        _mockCacheService.SetupGetCached($"{GameService.GameImageCacheKeyPrefix}{game.Key}", downloadContent);

        var result = await _gameService.GetGameImageAsync(game.Key);

        Assert.Equal(downloadContent, result);
    }

    [Fact]
    public async Task GetGameImageAsync_ShouldThrowGameNotFoundException_WhenGameDoesNotExist()
    {
        const string gameKey = "not-found";

        _mockCacheService.SetupGetNotCached<DownloadGameImageDto>($"{GameService.GameImageCacheKeyPrefix}{gameKey}");

        var act = () => _gameService.GetGameImageAsync(gameKey);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task GetGameImageAsync_ShouldThrowGameNotFoundException_WhenGameIsDeletedAndUserCannotViewDeletedGames()
    {
        var game = await SeedDeletedGameAsync();

        _mockCacheService.SetupGetNotCached<DownloadGameImageDto>($"{GameService.GameImageCacheKeyPrefix}{game.Key}");

        var act = () => _gameService.GetGameImageAsync(game.Key);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task GetGameImageAsync_ShouldReturnEmptyDownloadImageContent_WhenGameDoesNotHaveImageUrl()
    {
        var game = await SeedGameWithoutImageAsync();

        _mockCacheService.SetupGetNotCached<DownloadGameImageDto>($"{GameService.GameImageCacheKeyPrefix}{game.Key}");

        var result = await _gameService.GetGameImageAsync(game.Key);

        Assert.NotNull(result);
        Assert.Empty(result.Content);
    }

    [Fact]
    public async Task GetGameImageAsync_ShouldReturnDownloadImageContent_WhenGameKeyIsValid()
    {
        var game = await SeedGameAsync();

        _mockCacheService.SetupGetNotCached<DownloadGameImageDto>($"{GameService.GameImageCacheKeyPrefix}{game.Key}");

        var downloadContent = new DownloadGameImageDto
        {
            FileName = game.Key,
            ContentType = "image/png",
            Content = [1, 2, 3],
        };

        SetupMockKeyGameImageServiceDownloadByUrl(downloadContent);

        var result = await _gameService.GetGameImageAsync(game.Key);

        Assert.NotNull(result);
        Assert.Equivalent(downloadContent, result);
    }

    [Theory]
    [ClassData(typeof(InvalidEntityIdOrEmptyIdTestData))]
    public async Task GetByIdAsync_ShouldThrowArgumentException_WhenIdIsEmpty(string invalidId)
    {
        var act = () => _gameService.GetByIdAsync(invalidId);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowGameNotFoundException_WhenGameDoesNotExist()
    {
        var gameId = Guid.NewGuid();

        var act = () => _gameService.GetByIdAsync(gameId.ToString());

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowGameNotFoundException_WhenGameIsDeletedAndUserCannotViewDeletedGames()
    {
        var game = await SeedDeletedGameAsync();

        var act = () => _gameService.GetByIdAsync(game.Id.ToString());

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnGame_WhenIdIsValidGuid()
    {
        var game = await SeedGameAsync();

        var gameDto = _mapper.Map<GameDto>(game);

        var result = await _gameService.GetByIdAsync(game.Id.ToString());

        Assert.NotNull(result);
        Assert.Equivalent(gameDto, result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnGame_WhenIdIsValidIntId()
    {
        var game = await SeedGameAsync();

        var gameDto = _mapper.Map<GameDto>(game);

        var result = await _gameService.GetByIdAsync(game.ProductId.ToString()!);

        Assert.NotNull(result);
        Assert.Equivalent(gameDto, result);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task GetByKeyAsync_ShouldThrowArgumentException_WhenKeyIsInvalid(string invalidGameKey)
    {
        var act = () => _gameService.GetByKeyAsync(invalidGameKey);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetByKeyAsync_ShouldThrowGameNotFoundException_WhenGameDoesNotExist()
    {
        const string gameKey = "not-found";

        var act = () => _gameService.GetByKeyAsync(gameKey);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task GetByKeyAsync_ShouldThrowGameNotFoundException_WhenGameIsDeletedAndUserCannotViewDeletedGames()
    {
        var game = await SeedDeletedGameAsync();

        var act = () => _gameService.GetByKeyAsync(game.Key);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task GetByKeyAsync_ShouldReturnGame_WhenKeyIsValid()
    {
        var game = await SeedGameAsync();

        var gameDto = _mapper.Map<GameDto>(game);

        var result = await _gameService.GetByKeyAsync(game.Key);

        Assert.NotNull(result);
        Assert.Equivalent(gameDto, result);
    }

    [Fact]
    public async Task GetTotalCountAsync_ShouldReturnGameCountFromCache_WhenGamesExistInCache()
    {
        const int expectedCount = 10;

        _mockCacheService.SetupGetCached(GameService.TotalGameCountCacheKey, expectedCount);

        var result = await _gameService.GetTotalCountAsync();

        Assert.Equal(expectedCount, result);
    }

    [Fact]
    public async Task GetTotalCountAsync_ShouldReturnGameCountFromDatabase_WhenGamesNotExistInCache()
    {
        var games = await SeedGamesAsync();
        var expectedCount = games.Count(g => !g.IsDeleted);

        _mockCacheService.SetupGetNotCached<int>(GameService.TotalGameCountCacheKey);

        var result = await _gameService.GetTotalCountAsync();

        Assert.Equal(expectedCount, result);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task DownloadFileAsync_ShouldThrowArgumentException_WhenKeyIsInvalid(string invalidGameKey)
    {
        var act = () => _gameService.DownloadFileAsync(invalidGameKey);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task DownloadFileAsync_ShouldThrowGameNotFoundException_WhenGameDoesNotExist()
    {
        const string gameKey = "not-found";

        var act = () => _gameService.DownloadFileAsync(gameKey);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task DownloadFileAsync_ShouldThrowGameNotFoundException_WhenGameIsDeletedAndUserCannotViewDeletedGames()
    {
        var game = await SeedDeletedGameAsync();

        var act = () => _gameService.DownloadFileAsync(game.Key);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task DownloadFileAsync_ShouldReturnFileContent_WhenGameExists()
    {
        var game = await SeedGameWithDependenciesAsync();

        var result = await _gameService.DownloadFileAsync(game.Key);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Content);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var request = GameTestData.GetInvalidCreateRequest();

        _mockValidationService.SetupValidationThrows(request);

        var act = () => _gameService.CreateAsync(request);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateGame_WhenRequestIsValidAndKeyNotEmpty()
    {
        await SeedGameDependenciesAsync();

        const string gameKey = "game-key";
        const string imageUrl = "http://example.com/image.png";
        var request = GameTestData.GetCreateRequest(gameKey);
        var gameCreate = request.Game;

        SetupMockKeyGameImageServiceUpload(imageUrl);

        var result = await _gameService.CreateAsync(request);

        Assert.NotNull(result);

        var gameWithDetails = await GetGameWithDetailsAsync(result.Id);

        Assert.NotNull(gameWithDetails);
        Assert.Equal(gameKey, gameWithDetails.Key);
        Assert.Equal(imageUrl, gameWithDetails.ImageUrl);
        Assert.Equal(gameCreate.Name, gameWithDetails.Name);
        Assert.Equal(gameCreate.Description, gameWithDetails.Description);
        Assert.Equal(gameCreate.Price, gameWithDetails.Price);
        Assert.Equivalent(request.Genres.Select(x => x.ToString()).ToList(), gameWithDetails.GameGenres.Select(gg => gg.GenreId.ToString()).ToList());
        Assert.Equivalent(request.Platforms, gameWithDetails.GamePlatforms.Select(gp => gp.PlatformId).ToList());
        Assert.Equal(request.Publisher.ToString(), gameWithDetails.PublisherId.ToString());
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateGameAndGenerateUniqueKey_WhenRequestIsValidAndKeyEmpty()
    {
        await SeedGameDependenciesAsync();

        var gameKey = string.Empty;
        const string imageUrl = "http://example.com/image.png";
        var request = GameTestData.GetCreateRequest(gameKey);

        const string expectedKey = "new-game";

        SetupMockKeyGeneratorGenerateUniqueKey(request.Game.Name, expectedKey);
        SetupMockKeyGameImageServiceUpload(imageUrl);

        var result = await _gameService.CreateAsync(request);

        Assert.NotNull(result);

        var gameWithDetails = await GetGameWithDetailsAsync(result.Id);

        Assert.NotNull(gameWithDetails);
        Assert.Equal(expectedKey, gameWithDetails.Key);
        Assert.Equal(imageUrl, gameWithDetails.ImageUrl);
        Assert.Equivalent(request.Genres.Select(x => x.ToString()).ToList(), gameWithDetails.GameGenres.Select(gg => gg.GenreId.ToString()).ToList());
        Assert.Equivalent(request.Platforms, gameWithDetails.GamePlatforms.Select(gp => gp.PlatformId).ToList());
        Assert.Equal(request.Publisher.ToString(), gameWithDetails.PublisherId.ToString());
    }

    [Theory]
    [ClassData(typeof(InvalidEntityIdTestData))]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenIdIsInvalid(string invalidId)
    {
        var request = GameTestData.GetUpdateRequest(invalidId);

        var act = () => _gameService.UpdateAsync(request);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowGameNotFoundException_WhenGameDoesNotExist()
    {
        var request = GameTestData.GetUpdateRequest();

        var act = () => _gameService.UpdateAsync(request);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowGameNotFoundException_WhenGameIsDeletedAndAndUserCannotViewDeletedGames()
    {
        var game = await SeedDeletedGameAsync();

        var request = GameTestData.GetUpdateRequest(game.Id.ToString());

        var act = () => _gameService.UpdateAsync(request);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var game = await SeedGameAsync();

        var request = GameTestData.GetInvalidUpdateRequest(game.Id);

        _mockValidationService.SetupValidationThrows(request);

        var act = () => _gameService.UpdateAsync(request);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowForbiddenException_WhenGameIsDeletedAndUserCannotEditDeletedGames()
    {
        var game = await SeedDeletedGameAsync();

        var request = GameTestData.GetUpdateRequest(game.Id.ToString());

        _mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, true);
        _mockUserContext.SetupHasPermission(Permissions.EditDeletedGames, false);

        var act = () => _gameService.UpdateAsync(request);

        await Assert.ThrowsAsync<ForbiddenException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateGame_WhenRequestIsValidAndIdValidGuid()
    {
        await SeedGameDependenciesAsync();

        const string imageUrl = "http://example.com/image.png";

        var game = await SeedGameAsync();

        var request = GameTestData.GetUpdateRequest(game.Id.ToString());
        var gameUpdate = request.Game;

        SetupMockKeyGameImageServiceUpload(imageUrl);

        await _gameService.UpdateAsync(request);

        var gameWithDetails = await GetGameWithDetailsAsync(game.Id);

        Assert.NotNull(gameWithDetails);
        Assert.Equal(gameUpdate.Key, gameWithDetails.Key);
        Assert.Equal(gameUpdate.Name, gameWithDetails.Name);
        Assert.Equal(gameUpdate.Description, gameWithDetails.Description);
        Assert.Equal(gameUpdate.Price, gameWithDetails.Price);
        Assert.Equivalent(request.Genres.Select(x => x.ToString()).ToList(), gameWithDetails.GameGenres.Select(gg => gg.GenreId.ToString()).ToList());
        Assert.Equivalent(request.Platforms, gameWithDetails.GamePlatforms.Select(gp => gp.PlatformId).ToList());
        Assert.Equal(request.Publisher.ToString(), gameWithDetails.PublisherId.ToString());
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateGame_WhenRequestIsValidAndIdValidIntId()
    {
        await SeedGameDependenciesAsync();

        const string imageUrl = "http://example.com/image.png";

        var game = await SeedGameAsync();

        var request = GameTestData.GetUpdateRequest(game.ProductId.ToString());
        var gameUpdate = request.Game;

        SetupMockKeyGameImageServiceUpload(imageUrl);

        await _gameService.UpdateAsync(request);

        var gameWithDetails = await GetGameWithDetailsAsync(game.ProductId!.Value);

        Assert.NotNull(gameWithDetails);
        Assert.Equal(gameUpdate.Key, gameWithDetails.Key);
        Assert.Equal(gameUpdate.Name, gameWithDetails.Name);
        Assert.Equal(gameUpdate.Description, gameWithDetails.Description);
        Assert.Equal(gameUpdate.Price, gameWithDetails.Price);
        Assert.Equivalent(request.Genres.Select(x => x.ToString()).ToList(), gameWithDetails.GameGenres.Select(gg => gg.GenreId.ToString()).ToList());
        Assert.Equivalent(request.Platforms, gameWithDetails.GamePlatforms.Select(gp => gp.PlatformId).ToList());
        Assert.Equal(request.Publisher.ToString(), gameWithDetails.PublisherId.ToString());
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task DeleteByKeyAsync_ShouldThrowArgumentException_WhenKeyIsInvalid(string invalidGameKey)
    {
        var act = () => _gameService.DeleteByKeyAsync(invalidGameKey);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task DeleteByKeyAsync_ShouldThrowGameNotFoundException_WhenGameDoesNotExist()
    {
        const string gameKey = "not-found";

        var act = () => _gameService.DeleteByKeyAsync(gameKey);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task DeleteByKeyAsync_ShouldThrowGameNotFoundException_WhenGameIsSoftDeleted()
    {
        var game = await SeedDeletedGameAsync();

        var act = () => _gameService.DeleteByKeyAsync(game.Key);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task DeleteByKeyAsync_ShouldThrowConflictException_WhenGameIsDeletedAndUserCanViewDeletedGames()
    {
        var game = await SeedDeletedGameAsync();

        _mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, true);

        var act = () => _gameService.DeleteByKeyAsync(game.Key);

        await Assert.ThrowsAsync<ConflictException>(act);
    }

    [Fact]
    public async Task DeleteByKeyAsync_ShouldSoftDeleteGame_WhenKeyIsValid()
    {
        var game = await SeedGameWithDependenciesAsync();

        _mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, true);

        await _gameService.DeleteByKeyAsync(game.Key);

        var softDeletedGame = await _gameRepository.GetAsync(g => g.Key == game.Key);

        Assert.NotNull(softDeletedGame);
        Assert.True(game.IsDeleted);

        _mockGameImageService.Verify(
            x => x.DeleteAsync(It.Is<string>(name => name == game.Key), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void SetupMockKeyGeneratorGenerateUniqueKey(string gameName, string key)
    {
        _mockGameKeyGenerator.Setup(x => x.GenerateUniqueAsync(
                gameName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(key);
    }

    private void SetupMockKeyGameImageServiceUpload(string resultUrl)
    {
        _mockGameImageService.Setup(x => x.UploadAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultUrl);
    }

    private void SetupMockKeyGameImageServiceDownloadByUrl(DownloadGameImageDto downloadContent)
    {
        _mockGameImageService.Setup(x => x.DownloadByUrlAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(downloadContent);
    }

    private Task<Game?> GetGameWithDetailsAsync(Guid id)
    {
        return _gameRepository.GetByIdAsync(
            id,
            include: x => x
                .Include(g => g.GameGenres)
                .Include(g => g.GamePlatforms));
    }

    private Task<Game?> GetGameWithDetailsAsync(int id)
    {
        return _gameRepository.GetAsync(
            g => g.ProductId == id,
            include: x => x
                .Include(g => g.GameGenres)
                .Include(g => g.GamePlatforms));
    }

    private async Task<Game> SeedDeletedGameAsync()
    {
        var game = GameTestData.GetGame();
        game.IsDeleted = true;
        await _gameRepository.AddAsync(game);
        await _unitOfWork.SaveChangesAsync();
        return game;
    }

    private async Task<Game> SeedGameWithoutImageAsync()
    {
        var game = GameTestData.GetGame();
        game.ImageUrl = null;
        await _gameRepository.AddAsync(game);
        await _unitOfWork.SaveChangesAsync();
        return game;
    }

    private async Task<Game> SeedGameAsync()
    {
        var game = GameTestData.GetGame();
        await _gameRepository.AddAsync(game);
        await _unitOfWork.SaveChangesAsync();
        return game;
    }

    private async Task<Game> SeedGameWithDependenciesAsync()
    {
        var game = GameTestData.GetGame();
        var genres = GameTestData.GetGameGenres(game);
        var platforms = GameTestData.GetGamePlatforms(game);
        var publisher = PublisherTestData.GetPublisherById(game.PublisherId);

        await _genreRepository.AddRangeAsync(genres);
        await _platformRepository.AddRangeAsync(platforms);
        await _publisherRepository.AddAsync(publisher);
        await _gameRepository.AddAsync(game);
        await _unitOfWork.SaveChangesAsync();

        return game;
    }

    private async Task<List<Game>> SeedGamesAsync()
    {
        var games = GameTestData.GetGames();
        await _gameRepository.AddRangeAsync(games);
        await _unitOfWork.SaveChangesAsync();
        return games;
    }

    private async Task<(Game Game, List<Genre> Genres)> SeedGameWithGenresAsync()
    {
        var game = GameTestData.GetGame();
        var genres = GameTestData.GetGameGenres(game);

        await _genreRepository.AddRangeAsync(genres);
        await _gameRepository.AddAsync(game);
        await _unitOfWork.SaveChangesAsync();

        return (game, genres);
    }

    private async Task<(Game Game, List<Platform> Platforms)> SeedGameWithPlatformsAsync()
    {
        var game = GameTestData.GetGame();
        var platforms = GameTestData.GetGamePlatforms(game);

        await _platformRepository.AddRangeAsync(platforms);
        await _gameRepository.AddAsync(game);
        await _unitOfWork.SaveChangesAsync();

        return (game, platforms);
    }

    private async Task<(Game Game, Publisher Publisher)> SeedGameWithPublisherAsync()
    {
        var game = GameTestData.GetGame();
        var publisher = PublisherTestData.GetPublisherById(game.PublisherId);

        await _publisherRepository.AddAsync(publisher);
        await _gameRepository.AddAsync(game);
        await _unitOfWork.SaveChangesAsync();

        return (game, publisher);
    }

    private async Task SeedGameDependenciesAsync()
    {
        var genres = GenreTestData.GetGenres();
        var platforms = PlatformTestData.GetPlatforms();
        var publishers = PublisherTestData.GetPublishers();

        await _genreRepository.AddRangeAsync(genres);
        await _platformRepository.AddRangeAsync(platforms);
        await _publisherRepository.AddRangeAsync(publishers);
        await _unitOfWork.SaveChangesAsync();
    }
}