using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Games.Genres;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.BLL.Validators.Games.Genres;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Shared;

namespace Gamestore.BLL.Tests.Validators.Games.Genres;

public class BaseGenreDtoValidatorTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Genre> _genreRepository;
    private readonly BaseGenreDtoValidator _validator;

    public BaseGenreDtoValidatorTests()
    {
        _unitOfWork = UnitOfWorkFactory.Create();
        _genreRepository = _unitOfWork.Repositories.GetGeneric<Genre>();
        _validator = new BaseGenreDtoValidator(_genreRepository);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task Should_HaveError_When_NameIsEmpty(string invalidName)
    {
        var dto = new GenreCreateDto { Name = invalidName };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_NameIsTooShort()
    {
        var dto = new GenreCreateDto
        {
            Name = new string('a', GenreValidationRules.MinNameLength - 1),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage($"Name must be at least {GenreValidationRules.MinNameLength} characters long.");
    }

    [Fact]
    public async Task Should_HaveError_When_NameIsTooLong()
    {
        var dto = new GenreCreateDto
        {
            Name = new string('a', GenreValidationRules.MaxNameLength + 1),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage($"Name must not exceed {GenreValidationRules.MaxNameLength} characters.");
    }

    [Theory]
    [MemberData(nameof(GenreNamesWithInvalidCharacters))]
    public async Task Should_HaveError_When_NameHasInvalidCharacters(string invalidName)
    {
        var dto = new GenreCreateDto { Name = invalidName };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(GenreValidationRules.NamePatternMessage);
    }

    [Theory]
    [MemberData(nameof(ValidGenreNames))]
    public async Task Should_NotHaveError_When_NameIsValid(string name)
    {
        var dto = new GenreCreateDto { Name = name };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Should_HaveError_When_ParentGenreIdDoesNotExist()
    {
        var dto = new GenreCreateDto { ParentGenreId = new EntityId(Guid.NewGuid()) };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.ParentGenreId)
            .WithErrorMessage($"Parent genre with Id {dto.ParentGenreId} does not exist.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_ParentGenreIdExists()
    {
        var existingParentGenre = await SeedGenreAsync();

        var dto = new GenreCreateDto { ParentGenreId = new EntityId(existingParentGenre.Id) };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.ParentGenreId);
    }

    [Fact]
    public async Task Should_NotHaveError_When_ParentGenreIdIsEmpty()
    {
        var dto = new GenreCreateDto { ParentGenreId = null };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.ParentGenreId);
    }

    [Fact]
    public async Task Should_NotHaveError_When_AllValid()
    {
        var existingParentGenre = await SeedGenreAsync();

        var dto = new GenreCreateDto
        {
            Name = "Valid Name",
            ParentGenreId = new EntityId(existingParentGenre.Id),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    public static TheoryData<string> GenreNamesWithInvalidCharacters()
    {
        return new TheoryData<string>
        {
            "Action#Adventure",
            "Strategy!",
            "Role@Playing",
            "Survival $",
            "Horror%",
            "RPG^Quest",
            "Shooter*Game",
            "Platformer+Puzzle",
            "FPS~Action",
            "Adventure\\RPG",
        };
    }

    public static TheoryData<string> ValidGenreNames()
    {
        return new TheoryData<string>
        {
            "Action-Adventure",
            "Role-Playing Game",
            "First-Person Shooter",
            "Strategy & Tactics",
            "Open World RPG",
            "Survival Horror",
            "Massively Multiplayer Online",
            "Platformer",
            "Puzzle",
            "Stealth",
            "Sandbox",
            "Real-Time Strategy",
            "Turn-Based Strategy",
            "Fighting",
            "Roguelike",
            "Metroidvania",
            "Beat 'em Up",
            "Visual Novel",
            "Bullet Hell",
            "Simulation",
            "FPS/Action",
        };
    }

    private async Task<Genre> SeedGenreAsync()
    {
        var genre = GenreTestData.GetGenre();
        await _genreRepository.AddAsync(genre);
        await _unitOfWork.SaveChangesAsync();
        return genre;
    }
}