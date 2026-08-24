using FluentValidation;
using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.DTOs.Games.Platforms;
using Gamestore.BLL.Interfaces;
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
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Services.Games;

public class PlatformServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Platform> _platformRepository;
    private readonly IRepository<Game> _gameRepository;
    private readonly IMapper _mapper;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly PlatformService _platformService;

    public PlatformServiceTests()
    {
        _mockUserContext = new Mock<IUserContext>();
        _mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, false);

        _unitOfWork = UnitOfWorkFactory.Create(_mockUserContext.Object);
        _platformRepository = _unitOfWork.Repositories.GetGeneric<Platform>();
        _gameRepository = _unitOfWork.Repositories.GetGeneric<Game>();

        _mapper = MapperFactory.Create();
        _mockValidationService = new Mock<IValidationService>();

        var mockEntityChangeLogService = new Mock<IEntityChangeLogService>();

        _platformService = new PlatformService(
            new ServiceContext(_unitOfWork, _mapper, _mockValidationService.Object, _mockUserContext.Object),
            mockEntityChangeLogService.Object,
            Mock.Of<ILogger<PlatformService>>());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPlatforms_WhenPlatformsExist()
    {
        var platforms = await SeedPlatformsAsync();

        var platformDtos = _mapper.Map<List<PlatformDto>>(platforms);

        var result = await _platformService.GetAllAsync();

        Assert.NotEmpty(result);
        Assert.Equivalent(platformDtos, result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoPlatformsExist()
    {
        var result = await _platformService.GetAllAsync();

        Assert.Empty(result);
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task GetPlatformGamesAsync_ShouldThrowArgumentException_WhenPlatformIdIsInvalid(Guid invalidPlatformId)
    {
        var act = () => _platformService.GetPlatformGamesAsync(invalidPlatformId);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetPlatformGamesAsync_ShouldReturnDeletedGames_WhenUserCanViewDeletedGames()
    {
        var (platform, games) = await SeedPlatformWithGamesAsync();
        await UpdateGameAsDeletedAsync(games[0]);

        var gameDtos = _mapper.Map<List<GameDto>>(games);

        _mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, true);

        var result = await _platformService.GetPlatformGamesAsync(platform.Id);

        Assert.NotEmpty(result);
        Assert.Equivalent(gameDtos, result);
    }

    [Fact]
    public async Task GetPlatformGamesAsync_ShouldReturnGames_WhenPlatformIdIsValid()
    {
        var (platform, games) = await SeedPlatformWithGamesAsync();

        games = games.Where(g => !g.IsDeleted).ToList();

        var gameDtos = _mapper.Map<List<GameDto>>(games);

        var result = await _platformService.GetPlatformGamesAsync(platform.Id);

        Assert.NotEmpty(result);
        Assert.Equivalent(gameDtos, result);
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task GetByIdAsync_ShouldThrowArgumentException_WhenIdIsInvalid(Guid invalidId)
    {
        var act = () => _platformService.GetByIdAsync(invalidId);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenPlatformDoesNotExist()
    {
        var platformId = Guid.NewGuid();

        var act = () => _platformService.GetByIdAsync(platformId);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPlatform_WhenIdIsValid()
    {
        var platform = await SeedPlatformAsync();

        var platformDto = _mapper.Map<PlatformDto>(platform);

        var result = await _platformService.GetByIdAsync(platform.Id);

        Assert.NotNull(result);
        Assert.Equivalent(platformDto, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var request = PlatformTestData.GetInvalidCreateRequest();

        _mockValidationService.SetupValidationThrows(request);

        var act = () => _platformService.CreateAsync(request);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePlatform_WhenRequestIsValid()
    {
        var request = PlatformTestData.GetCreateRequest();

        var result = await _platformService.CreateAsync(request);

        Assert.NotNull(result);

        var platform = await GetPlatformAsync(result.Id);

        Assert.NotNull(platform);
        Assert.Equivalent(request.Platform, platform);
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenIdIsInvalid(Guid invalidId)
    {
        var request = PlatformTestData.GetUpdateRequest(invalidId);

        var act = () => _platformService.UpdateAsync(request);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenPlatformDoesNotExist()
    {
        var request = PlatformTestData.GetUpdateRequest();

        var act = () => _platformService.UpdateAsync(request);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var platform = await SeedPlatformAsync();

        var request = PlatformTestData.GetInvalidUpdateRequest(platform.Id);

        _mockValidationService.SetupValidationThrows(request);

        var act = () => _platformService.UpdateAsync(request);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePlatform_WhenRequestIsValid()
    {
        var platform = await SeedPlatformAsync();

        var request = PlatformTestData.GetUpdateRequest(platform.Id);

        await _platformService.UpdateAsync(request);

        var updatedPlatform = await GetPlatformAsync(platform.Id);

        Assert.NotNull(updatedPlatform);
        Assert.Equivalent(request.Platform, updatedPlatform);
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task DeleteAsync_ShouldThrowArgumentException_WhenIdIsInvalid(Guid invalidId)
    {
        var act = () => _platformService.DeleteAsync(invalidId);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenPlatformDoesNotExist()
    {
        var platformId = Guid.NewGuid();

        var act = () => _platformService.DeleteAsync(platformId);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeletePlatform_WhenIdIsValid()
    {
        var platform = await SeedPlatformAsync();

        await _platformService.DeleteAsync(platform.Id);

        Assert.False(await PlatformExistsAsync(platform.Id));
    }

    private Task<bool> PlatformExistsAsync(Guid id)
    {
        return _platformRepository.ExistsAsync(p => p.Id == id);
    }

    private Task<Platform?> GetPlatformAsync(Guid id)
    {
        return _platformRepository.GetByIdAsync(id);
    }

    private async Task<Platform> SeedPlatformAsync()
    {
        var platform = PlatformTestData.GetPlatform();
        await _platformRepository.AddAsync(platform);
        await _unitOfWork.SaveChangesAsync();
        return platform;
    }

    private async Task<List<Platform>> SeedPlatformsAsync()
    {
        var platforms = PlatformTestData.GetPlatforms();
        await _platformRepository.AddRangeAsync(platforms);
        await _unitOfWork.SaveChangesAsync();
        return platforms;
    }

    private async Task<(Platform Platform, List<Game> Games)> SeedPlatformWithGamesAsync()
    {
        var platform = PlatformTestData.GetPlatform();
        var games = GameTestData.GetGamesByPlatform(platform.Id);

        await _platformRepository.AddAsync(platform);
        await _gameRepository.AddRangeAsync(games);
        await _unitOfWork.SaveChangesAsync();

        return (platform, games);
    }

    private async Task UpdateGameAsDeletedAsync(Game game)
    {
        game.IsDeleted = true;
        await _gameRepository.UpdateAsync(game);
        await _unitOfWork.SaveChangesAsync();
    }
}