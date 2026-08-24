using FluentValidation;
using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.DTOs.Games.Genres;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Services.Games;

public class GenreServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Genre> _genreRepository;
    private readonly IRepository<Game> _gameRepository;
    private readonly IMapper _mapper;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly GenreService _genreService;

    public GenreServiceTests()
    {
        _mockUserContext = new Mock<IUserContext>();
        _mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, false);

        _unitOfWork = UnitOfWorkFactory.Create(_mockUserContext.Object);
        _genreRepository = _unitOfWork.Repositories.GetGeneric<Genre>();
        _gameRepository = _unitOfWork.Repositories.GetGeneric<Game>();

        _mapper = MapperFactory.Create();
        _mockValidationService = new Mock<IValidationService>();

        var mockEntityChangeLogService = new Mock<IEntityChangeLogService>();

        _genreService = new GenreService(
            new ServiceContext(_unitOfWork, _mapper, _mockValidationService.Object, _mockUserContext.Object),
            mockEntityChangeLogService.Object,
            Mock.Of<ILogger<GenreService>>());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnGenres_WhenGenresExist()
    {
        var genres = await SeedGenresAsync();

        var genreDtos = _mapper.Map<List<GenreDto>>(genres);

        var result = await _genreService.GetAllAsync();

        Assert.NotEmpty(result);
        Assert.Equivalent(genreDtos, result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoGenresExist()
    {
        var result = await _genreService.GetAllAsync();

        Assert.Empty(result);
    }

    [Theory]
    [ClassData(typeof(InvalidEntityIdOrEmptyIdTestData))]
    public async Task GetSubGenresAsync_ShouldThrowArgumentException_WhenParentIdIsInvalid(string invalidParentId)
    {
        var act = () => _genreService.GetSubGenresAsync(invalidParentId);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetSubGenresAsync_ShouldReturnSubGenres_WhenParentIdIsValid()
    {
        var parentGenre = await SeedGenreWithSubGenresAsync();

        var subGenreDtos = _mapper.Map<List<GenreDto>>(parentGenre.SubGenres);

        var result = await _genreService.GetSubGenresAsync(parentGenre.Id.ToString());

        Assert.NotEmpty(result);
        Assert.Equivalent(subGenreDtos, result);
    }

    [Fact]
    public async Task GetSubGenresAsync_ShouldReturnEmptyList_WhenNoSubGenresExistForParent()
    {
        var parentGenreId = Guid.NewGuid();

        var result = await _genreService.GetSubGenresAsync(parentGenreId.ToString());

        Assert.Empty(result);
    }

    [Theory]
    [ClassData(typeof(InvalidEntityIdOrEmptyIdTestData))]
    public async Task GetGenreGamesAsync_ShouldThrowArgumentException_WhenGenreIdIsInvalid(string invalidGenreId)
    {
        var act = () => _genreService.GetGenreGamesAsync(invalidGenreId);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetGenreGamesAsync_ShouldReturnDeletedGames_WhenUserCanViewDeletedGames()
    {
        var (genre, games) = await SeedGenreWithGamesAsync();
        await UpdateGameAsDeletedAsync(games[0]);

        var gameDtos = _mapper.Map<List<GameDto>>(games);

        _mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, true);

        var result = await _genreService.GetGenreGamesAsync(genre.Id.ToString());

        Assert.NotEmpty(result);
        Assert.Equivalent(gameDtos, result);
    }

    [Fact]
    public async Task GetGenreGamesAsync_ShouldReturnGames_WhenGenreIdIsValidGuid()
    {
        var (genre, games) = await SeedGenreWithGamesAsync();
        games = games.Where(g => !g.IsDeleted).ToList();

        var gameDtos = _mapper.Map<List<GameDto>>(games);

        var result = await _genreService.GetGenreGamesAsync(genre.Id.ToString());

        Assert.NotEmpty(result);
        Assert.Equivalent(gameDtos, result);
    }

    [Fact]
    public async Task GetGenreGamesAsync_ShouldReturnGames_WhenGenreIdIsValidIntId()
    {
        var (category, games) = await SeedCategoryWithGamesAsync();
        games = games.Where(g => !g.IsDeleted).ToList();

        var gameDtos = _mapper.Map<List<GameDto>>(games);

        var result = await _genreService.GetGenreGamesAsync(category.CategoryId.ToString()!);

        Assert.NotEmpty(result);
        Assert.Equivalent(gameDtos, result);
    }

    [Theory]
    [ClassData(typeof(InvalidEntityIdOrEmptyIdTestData))]
    public async Task GetByIdAsync_ShouldThrowArgumentException_WhenIdIsInvalid(string invalidId)
    {
        var act = () => _genreService.GetByIdAsync(invalidId);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenGenreDoesNotExist()
    {
        var genreId = Guid.NewGuid();

        var act = () => _genreService.GetByIdAsync(genreId.ToString());

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnGenre_WhenIdIsValidGuid()
    {
        var genre = await SeedGenreAsync();

        var genreDetailedDto = _mapper.Map<GenreDetailedDto>(genre);

        var result = await _genreService.GetByIdAsync(genre.Id.ToString());

        Assert.NotNull(result);
        Assert.Equivalent(genreDetailedDto, result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnGenre_WhenIdIsValidIntId()
    {
        var genre = await SeedGenreAsync();

        var genreDetailedDto = _mapper.Map<GenreDetailedDto>(genre);

        var result = await _genreService.GetByIdAsync(genre.CategoryId.ToString()!);

        Assert.NotNull(result);
        Assert.Equivalent(genreDetailedDto, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var request = GenreTestData.GetInvalidCreateRequest();

        _mockValidationService.SetupValidationThrows(request);

        var act = () => _genreService.CreateAsync(request);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateGenre_WhenRequestIsValid()
    {
        var request = GenreTestData.GetCreateRequest();

        var result = await _genreService.CreateAsync(request);

        Assert.NotNull(result);

        var genre = await GetGenreAsync(result.Id);

        Assert.NotNull(genre);
        Assert.Equivalent(request.Genre, genre);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateGenreWithParentGenre_WhenRequestIsValidAndParentGenreExists()
    {
        var request = GenreTestData.GetCreateRequestWithParentGenre();
        var genreCreate = request.Genre;
        var parentGenreId = genreCreate.ParentGenreId!.Value.PrimaryId;

        await SeedGenreAsync(parentGenreId);

        var result = await _genreService.CreateAsync(request);

        Assert.NotNull(result);

        var genre = await GetGenreWithParentAsync(result.Id);

        Assert.NotNull(genre);
        Assert.Equal(genreCreate.Name, genre.Name);
        Assert.Equal(parentGenreId, genre.ParentGenreId);
    }

    [Theory]
    [ClassData(typeof(InvalidEntityIdTestData))]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenIdIsInvalid(string invalidId)
    {
        var request = GenreTestData.GetUpdateRequest(invalidId);

        var act = () => _genreService.UpdateAsync(request);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenGenreDoesNotExist()
    {
        var request = GenreTestData.GetUpdateRequest();

        var act = () => _genreService.UpdateAsync(request);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var genre = await SeedGenreAsync();

        var request = GenreTestData.GetInvalidUpdateRequest(genre.Id);

        _mockValidationService.SetupValidationThrows(request);

        var act = () => _genreService.UpdateAsync(request);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateGenre_WhenRequestIsValidAndIdValidGuid()
    {
        var genre = await SeedGenreAsync();

        var request = GenreTestData.GetUpdateRequest(genre.Id.ToString());
        var genreUpdate = request.Genre;
        var parentGenreId = genreUpdate.ParentGenreId!.Value.PrimaryId;

        await SeedGenreAsync(parentGenreId);

        await _genreService.UpdateAsync(request);

        var updatedGenre = await GetGenreWithParentAsync(genre.Id);

        Assert.NotNull(updatedGenre);
        Assert.Equal(genreUpdate.Name, updatedGenre.Name);
        Assert.Equal(parentGenreId, updatedGenre.ParentGenreId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateGenre_WhenRequestIsValidAndIdValidIntId()
    {
        var genre = await SeedGenreAsync();

        var request = GenreTestData.GetUpdateRequest(genre.CategoryId.ToString());
        var genreUpdate = request.Genre;
        var parentGenreId = genreUpdate.ParentGenreId!.Value.PrimaryId;

        await SeedGenreAsync(parentGenreId);

        await _genreService.UpdateAsync(request);

        var updatedGenre = await GetGenreWithParentAsync(genre.CategoryId!.Value);

        Assert.NotNull(updatedGenre);
        Assert.Equal(genreUpdate.Name, updatedGenre.Name);
        Assert.Equal(parentGenreId, updatedGenre.ParentGenreId);
    }

    [Theory]
    [ClassData(typeof(InvalidEntityIdOrEmptyIdTestData))]
    public async Task DeleteAsync_ShouldThrowArgumentException_WhenIdIsInvalid(string invalidId)
    {
        var act = () => _genreService.DeleteAsync(invalidId);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenGenreDoesNotExist()
    {
        var genreId = Guid.NewGuid();

        var act = () => _genreService.DeleteAsync(genreId.ToString());

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteGenre_WhenIdIsValidGuid()
    {
        var genre = await SeedGenreAsync();

        await _genreService.DeleteAsync(genre.Id.ToString());

        Assert.False(await GenreExistsAsync(genre.Id));
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteGenre_WhenIdIsValidIntId()
    {
        var genre = await SeedGenreAsync();

        await _genreService.DeleteAsync(genre.CategoryId.ToString()!);

        Assert.False(await GenreExistsAsync(genre.CategoryId!.Value));
    }

    private Task<bool> GenreExistsAsync(Guid id)
    {
        return _genreRepository.ExistsAsync(g => g.Id == id);
    }

    private Task<bool> GenreExistsAsync(int id)
    {
        return _genreRepository.ExistsAsync(g => g.CategoryId == id);
    }

    private Task<Genre?> GetGenreAsync(Guid id)
    {
        return _genreRepository.GetByIdAsync(id);
    }

    private Task<Genre?> GetGenreWithParentAsync(Guid id)
    {
        return _genreRepository.GetSingleAsync(
            x => x.Id == id && x.ParentGenreId.HasValue,
            include: g => g.Include(genre => genre.ParentGenre!));
    }

    private Task<Genre?> GetGenreWithParentAsync(int id)
    {
        return _genreRepository.GetSingleAsync(
            x => x.CategoryId == id && x.ParentGenreId.HasValue,
            include: g => g.Include(genre => genre.ParentGenre!));
    }

    private async Task<Genre> SeedGenreAsync()
    {
        var genre = GenreTestData.GetGenre();
        await _genreRepository.AddAsync(genre);
        await _unitOfWork.SaveChangesAsync();
        return genre;
    }

    private async Task SeedGenreAsync(Guid? id)
    {
        if (!id.HasValue)
        {
            return;
        }

        var genre = GenreTestData.GetGenreById(id.Value);

        await _genreRepository.AddAsync(genre);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Genre> SeedGenreWithSubGenresAsync()
    {
        var genre = GenreTestData.GetGenreWithSubGenres();
        await _genreRepository.AddAsync(genre);
        await _unitOfWork.SaveChangesAsync();
        return genre;
    }

    private async Task<List<Genre>> SeedGenresAsync()
    {
        var genres = GenreTestData.GetGenres();
        await _genreRepository.AddRangeAsync(genres);
        await _unitOfWork.SaveChangesAsync();
        return genres;
    }

    private async Task<(Genre Genre, List<Game> Games)> SeedGenreWithGamesAsync()
    {
        var genre = GenreTestData.GetGenre();
        var games = GameTestData.GetGamesByGenre(genre.Id);

        await _genreRepository.AddAsync(genre);
        await _gameRepository.AddRangeAsync(games);
        await _unitOfWork.SaveChangesAsync();

        return (genre, games);
    }

    private async Task<(Genre Category, List<Game> Games)> SeedCategoryWithGamesAsync()
    {
        var category = GenreTestData.GetGenre();
        var games = GameTestData.GetGamesByCategory(category.CategoryId);

        await _genreRepository.AddAsync(category);
        await _gameRepository.AddRangeAsync(games);
        await _unitOfWork.SaveChangesAsync();

        return (category, games);
    }

    private async Task UpdateGameAsDeletedAsync(Game game)
    {
        game.IsDeleted = true;
        await _gameRepository.UpdateAsync(game);
        await _unitOfWork.SaveChangesAsync();
    }
}