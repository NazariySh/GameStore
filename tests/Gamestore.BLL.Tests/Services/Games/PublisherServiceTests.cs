using FluentValidation;
using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.DTOs.Games.Publishers;
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

public class PublisherServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Publisher> _publisherRepository;
    private readonly IRepository<Game> _gameRepository;
    private readonly IMapper _mapper;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly PublisherService _publisherService;

    public PublisherServiceTests()
    {
        _mockUserContext = new Mock<IUserContext>();
        _mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, false);

        _unitOfWork = UnitOfWorkFactory.Create(_mockUserContext.Object);
        _publisherRepository = _unitOfWork.Repositories.GetGeneric<Publisher>();
        _gameRepository = _unitOfWork.Repositories.GetGeneric<Game>();

        _mapper = MapperFactory.Create();
        _mockValidationService = new Mock<IValidationService>();

        var mockEntityChangeLogService = new Mock<IEntityChangeLogService>();

        _publisherService = new PublisherService(
            new ServiceContext(_unitOfWork, _mapper, _mockValidationService.Object, _mockUserContext.Object),
            mockEntityChangeLogService.Object,
            Mock.Of<ILogger<PublisherService>>());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPublishers_WhenPublishersExist()
    {
        var publishers = await SeedPublishersAsync();

        var publisherDtos = _mapper.Map<List<PublisherDto>>(publishers);

        var result = await _publisherService.GetAllAsync();

        Assert.NotEmpty(result);
        Assert.Equivalent(publisherDtos, result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoPublishersExist()
    {
        var result = await _publisherService.GetAllAsync();

        Assert.Empty(result);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task GetPublisherGamesAsync_ShouldThrowArgumentException_WhenCompanyNameIsInvalid(string invalidCompanyName)
    {
        var act = () => _publisherService.GetPublisherGamesAsync(invalidCompanyName);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetPublisherGamesAsync_ShouldThrowNotFoundException_WhenPublisherDoesNotExist()
    {
        const string companyName = "NonExistentPublisher";

        var act = () => _publisherService.GetPublisherGamesAsync(companyName);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task GetPublisherGamesAsync_ShouldReturnDeletedGames_WhenUserCanViewDeletedGames()
    {
        var (publisher, games) = await SeedPublisherWithGamesAsync();
        await UpdateGameAsDeletedAsync(games[0]);

        var gameDtos = _mapper.Map<List<GameDto>>(games);

        _mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, true);

        var result = await _publisherService.GetPublisherGamesAsync(publisher.CompanyName);

        Assert.NotEmpty(result);
        Assert.Equivalent(gameDtos, result);
    }

    [Fact]
    public async Task GetPublisherGamesAsync_ShouldReturnGames_WhenCompanyNameIsValid()
    {
        var (publisher, games) = await SeedPublisherWithGamesAsync();

        games = games.Where(g => !g.IsDeleted).ToList();

        var gameDtos = _mapper.Map<List<GameDto>>(games);

        var result = await _publisherService.GetPublisherGamesAsync(publisher.CompanyName);

        Assert.NotEmpty(result);
        Assert.Equivalent(gameDtos, result);
    }

    [Theory]
    [ClassData(typeof(InvalidEntityIdOrEmptyIdTestData))]
    public async Task GetByIdAsync_ShouldThrowArgumentException_WhenIdIsInvalid(string invalidId)
    {
        var act = () => _publisherService.GetByIdAsync(invalidId);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenPublisherDoesNotExist()
    {
        var publisherId = Guid.NewGuid();

        var act = () => _publisherService.GetByIdAsync(publisherId.ToString());

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPublisher_WhenIdIsValidGuid()
    {
        var publisher = await SeedPublisherAsync();

        var publisherDto = _mapper.Map<PublisherDto>(publisher);

        var result = await _publisherService.GetByIdAsync(publisher.Id.ToString());

        Assert.NotNull(result);
        Assert.Equivalent(publisherDto, result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPublisher_WhenIdIsValidIntId()
    {
        var publisher = await SeedPublisherAsync();

        var publisherDto = _mapper.Map<PublisherDto>(publisher);

        var result = await _publisherService.GetByIdAsync(publisher.SupplierId.ToString()!);

        Assert.NotNull(result);
        Assert.Equivalent(publisherDto, result);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task GetByCompanyNameAsync_ShouldThrowArgumentException_WhenCompanyNameIsInvalid(string invalidCompanyName)
    {
        var act = () => _publisherService.GetByCompanyNameAsync(invalidCompanyName);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetByCompanyNameAsync_ShouldThrowNotFoundException_WhenPublisherDoesNotExist()
    {
        const string companyName = "NonExistentPublisher";

        var act = () => _publisherService.GetByCompanyNameAsync(companyName);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task GetByCompanyNameAsync_ShouldReturnPublisher_WhenCompanyNameIsValid()
    {
        var publisher = await SeedPublisherAsync();

        var publisherDto = _mapper.Map<PublisherDto>(publisher);

        var result = await _publisherService.GetByCompanyNameAsync(publisher.CompanyName);

        Assert.NotNull(result);
        Assert.Equivalent(publisherDto, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var request = PublisherTestData.GetInvalidCreateRequest();

        _mockValidationService.SetupValidationThrows(request);

        var act = () => _publisherService.CreateAsync(request);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePublisher_WhenRequestIsValid()
    {
        var request = PublisherTestData.GetCreateRequest();
        var publisherCreate = request.Publisher;

        var result = await _publisherService.CreateAsync(request);

        Assert.NotNull(result);

        var publisher = await GetPublisherAsync(result.Id);

        Assert.NotNull(publisher);
        Assert.Equal(publisherCreate.CompanyName, publisher.CompanyName);
        Assert.Equal(publisherCreate.Description, publisher.Description);
        Assert.Equal(publisherCreate.HomePage, publisher.HomePage);
    }

    [Theory]
    [ClassData(typeof(InvalidEntityIdTestData))]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenIdIsInvalid(string invalidId)
    {
        var request = PublisherTestData.GetUpdateRequest(invalidId);

        var act = () => _publisherService.UpdateAsync(request);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenPublisherDoesNotExist()
    {
        var request = PublisherTestData.GetUpdateRequest();

        var act = () => _publisherService.UpdateAsync(request);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var publisher = await SeedPublisherAsync();

        var request = PublisherTestData.GetInvalidUpdateRequest(publisher.Id);

        _mockValidationService.SetupValidationThrows(request);

        var act = () => _publisherService.UpdateAsync(request);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePublisher_WhenRequestIsValidAndIdValidGuid()
    {
        var publisher = await SeedPublisherAsync();

        var request = PublisherTestData.GetUpdateRequest(publisher.Id.ToString());
        var publisherUpdate = request.Publisher;

        await _publisherService.UpdateAsync(request);

        var updatedPublisher = await GetPublisherAsync(publisher.Id);

        Assert.NotNull(updatedPublisher);
        Assert.Equal(publisherUpdate.CompanyName, updatedPublisher.CompanyName);
        Assert.Equal(publisherUpdate.Description, updatedPublisher.Description);
        Assert.Equal(publisherUpdate.HomePage, updatedPublisher.HomePage);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePublisher_WhenRequestIsValidAndIdValidIntId()
    {
        var publisher = await SeedPublisherAsync();

        var request = PublisherTestData.GetUpdateRequest(publisher.SupplierId.ToString());
        var publisherUpdate = request.Publisher;

        await _publisherService.UpdateAsync(request);

        var updatedPublisher = await _publisherRepository.GetAsync(p => p.SupplierId == publisher.SupplierId);

        Assert.NotNull(updatedPublisher);
        Assert.Equal(publisherUpdate.CompanyName, updatedPublisher.CompanyName);
        Assert.Equal(publisherUpdate.Description, updatedPublisher.Description);
        Assert.Equal(publisherUpdate.HomePage, updatedPublisher.HomePage);
    }

    [Theory]
    [ClassData(typeof(InvalidEntityIdOrEmptyIdTestData))]
    public async Task DeleteAsync_ShouldThrowArgumentException_WhenIdIsInvalid(string invalidId)
    {
        var act = () => _publisherService.DeleteAsync(invalidId);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenPublisherDoesNotExist()
    {
        var publisherId = Guid.NewGuid();

        var act = () => _publisherService.DeleteAsync(publisherId.ToString());

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowConflictException_WhenPublisherHasGames()
    {
        var (publisher, _) = await SeedPublisherWithGamesAsync();

        var act = () => _publisherService.DeleteAsync(publisher.Id.ToString());

        await Assert.ThrowsAsync<ConflictException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeletePublisher_WhenIdIsValidGuid()
    {
        var publisher = await SeedPublisherAsync();

        await _publisherService.DeleteAsync(publisher.Id.ToString());

        Assert.False(await PublisherExistsAsync(publisher.Id));
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeletePublisher_WhenIdIsValidIntId()
    {
        var publisher = await SeedPublisherAsync();

        await _publisherService.DeleteAsync(publisher.SupplierId.ToString()!);

        Assert.False(await PublisherExistsAsync(publisher.SupplierId!.Value));
    }

    private Task<bool> PublisherExistsAsync(Guid id)
    {
        return _publisherRepository.ExistsAsync(p => p.Id == id);
    }

    private Task<bool> PublisherExistsAsync(int id)
    {
        return _publisherRepository.ExistsAsync(p => p.SupplierId == id);
    }

    private Task<Publisher?> GetPublisherAsync(Guid id)
    {
        return _publisherRepository.GetByIdAsync(id);
    }

    private async Task<Publisher> SeedPublisherAsync()
    {
        var publisher = PublisherTestData.GetPublisher();
        await _publisherRepository.AddAsync(publisher);
        await _unitOfWork.SaveChangesAsync();
        return publisher;
    }

    private async Task<List<Publisher>> SeedPublishersAsync()
    {
        var publishers = PublisherTestData.GetPublishers();
        await _publisherRepository.AddRangeAsync(publishers);
        await _unitOfWork.SaveChangesAsync();
        return publishers;
    }

    private async Task<(Publisher Publisher, List<Game> Games)> SeedPublisherWithGamesAsync()
    {
        var publisher = PublisherTestData.GetPublisher();
        var games = GameTestData.GetGamesByPublisher(publisher.Id);

        await _publisherRepository.AddAsync(publisher);
        await _gameRepository.AddRangeAsync(games);
        await _unitOfWork.SaveChangesAsync();

        return (publisher, games);
    }

    private async Task UpdateGameAsDeletedAsync(Game game)
    {
        game.IsDeleted = true;
        await _gameRepository.UpdateAsync(game);
        await _unitOfWork.SaveChangesAsync();
    }
}