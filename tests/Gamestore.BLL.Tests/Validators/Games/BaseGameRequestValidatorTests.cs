using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.BLL.Validators.Games;
using Gamestore.BLL.Validators.Images;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Shared;
using Moq;

namespace Gamestore.BLL.Tests.Validators.Games;

public class BaseGameRequestValidatorTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Genre> _genreRepository;
    private readonly IRepository<Platform> _platformRepository;
    private readonly IRepository<Publisher> _publisherRepository;
    private readonly BaseGameRequestValidator _validator;

    public BaseGameRequestValidatorTests()
    {
        _unitOfWork = UnitOfWorkFactory.Create();
        _genreRepository = _unitOfWork.Repositories.GetGeneric<Genre>();
        _platformRepository = _unitOfWork.Repositories.GetGeneric<Platform>();
        _publisherRepository = _unitOfWork.Repositories.GetGeneric<Publisher>();

        var mockImageValidator = new Mock<Base64ImageValidator>();

        _validator = new BaseGameRequestValidator(
            mockImageValidator.Object,
            _genreRepository,
            _platformRepository,
            _publisherRepository);
    }

    [Fact]
    public async Task Should_HaveError_When_EmptyGenres()
    {
        var request = new CreateGameRequest
        {
            Genres = new List<EntityId>(),
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Genres)
            .WithErrorMessage("At least one genre is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_GenreIdIsInvalid()
    {
        var request = new CreateGameRequest
        {
            Genres = new List<EntityId> { new() },
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor("Genres[0]")
            .WithErrorMessage("Genre Id is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_GenreIdDoesNotExist()
    {
        var genreId = Guid.NewGuid();
        var request = new CreateGameRequest
        {
            Genres = new List<EntityId> { new(genreId) },
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor("Genres[0]")
            .WithErrorMessage($"Genre with Id {genreId} does not exist.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_GenresAreValid()
    {
        var genre = await SeedGenreAsync();

        var request = new CreateGameRequest
        {
            Genres = new List<EntityId> { new(genre.Id) },
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Genres);
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task Should_HaveError_When_PlatformIdIsInvalid(Guid invalidId)
    {
        var request = new CreateGameRequest
        {
            Platforms = new List<Guid> { invalidId },
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor("Platforms[0]")
            .WithErrorMessage("Platform Id is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_PlatformIdDoesNotExist()
    {
        var platformId = Guid.NewGuid();
        var request = new CreateGameRequest
        {
            Platforms = new List<Guid> { platformId },
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor("Platforms[0]")
            .WithErrorMessage($"Platform with Id {platformId} does not exist.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_EmptyPlatforms()
    {
        var request = new CreateGameRequest
        {
            Platforms = new List<Guid>(),
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Platforms);
    }

    [Fact]
    public async Task Should_NotHaveError_When_PlatformsAreValid()
    {
        var platform = await SeedPlatformAsync();

        var request = new CreateGameRequest
        {
            Platforms = new List<Guid> { platform.Id },
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Platforms);
    }

    [Fact]
    public async Task Should_HaveError_When_PublisherIdIsInvalid()
    {
        var request = new CreateGameRequest
        {
            Publisher = new EntityId(),
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Publisher)
            .WithErrorMessage("Publisher is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_PublisherIdDoesNotExist()
    {
        var publisherId = Guid.NewGuid();
        var request = new CreateGameRequest
        {
            Publisher = new EntityId(publisherId),
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Publisher)
            .WithErrorMessage($"Publisher with Id {publisherId} does not exist.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_PublisherIsValid()
    {
        var publisher = await SeedPublisherAsync();

        var request = new CreateGameRequest
        {
            Publisher = new EntityId(publisher.Id),
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Publisher);
    }

    [Fact]
    public async Task Should_NotHaveError_When_AllIdsAreValid()
    {
        var genre = await SeedGenreAsync();
        var platform = await SeedPlatformAsync();
        var publisher = await SeedPublisherAsync();

        var request = new CreateGameRequest
        {
            Game = new GameCreateDto
            {
                Name = "Valid Game",
                Key = "valid-game-key",
                Description = "A valid game description.",
            },
            Genres = new List<EntityId> { new(genre.Id) },
            Platforms = new List<Guid> { platform.Id },
            Publisher = new EntityId(publisher.Id),
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private async Task<Genre> SeedGenreAsync()
    {
        var genre = GenreTestData.GetGenre();
        await _genreRepository.AddAsync(genre);
        await _unitOfWork.SaveChangesAsync();
        return genre;
    }

    private async Task<Platform> SeedPlatformAsync()
    {
        var platform = PlatformTestData.GetPlatform();
        await _platformRepository.AddAsync(platform);
        await _unitOfWork.SaveChangesAsync();
        return platform;
    }

    private async Task<Publisher> SeedPublisherAsync()
    {
        var publisher = PublisherTestData.GetPublisher();
        await _publisherRepository.AddAsync(publisher);
        await _unitOfWork.SaveChangesAsync();
        return publisher;
    }
}