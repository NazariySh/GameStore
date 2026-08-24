using FluentValidation;
using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Games.Genres;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.BLL.Validators.Games.Genres;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Shared;
using Moq;

namespace Gamestore.BLL.Tests.Validators.Games.Genres;

public class GenreUpdateDtoValidatorTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Genre> _genreRepository;
    private readonly Mock<IValidator<GenreCreateUpdateDto>> _mockBaseValidator;
    private readonly GenreUpdateDtoValidator _validator;

    public GenreUpdateDtoValidatorTests()
    {
        _unitOfWork = UnitOfWorkFactory.Create();
        _genreRepository = _unitOfWork.Repositories.GetGeneric<Genre>();
        _mockBaseValidator = new Mock<IValidator<GenreCreateUpdateDto>>();
        _validator = new GenreUpdateDtoValidator(
            _genreRepository,
            _mockBaseValidator.Object);
    }

    [Fact]
    public async Task Should_CallBaseValidator()
    {
        var dto = new GenreUpdateDto { Id = new EntityId(Guid.NewGuid()), Name = "Test Genre" };

        await _validator.ValidateAsync(dto);

        _mockBaseValidator.VerifyValidateCalledOnce(dto);
    }

    [Fact]
    public async Task Should_HaveError_When_NameIsNotUnique()
    {
        var existingGenre = await SeedGenreAsync();

        var dto = new GenreUpdateDto { Id = new EntityId(Guid.NewGuid()), Name = existingGenre.Name };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Genre with this name already exists.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_NameIsUnique()
    {
        var dto = new GenreUpdateDto { Id = new EntityId(Guid.NewGuid()), Name = "Unique Genre" };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Should_HaveError_When_ParentIdEqualsId()
    {
        var id = Guid.NewGuid();
        var dto = new GenreUpdateDto
        {
            Id = new EntityId(id),
            ParentGenreId = new EntityId(id),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.ParentGenreId)
            .WithErrorMessage("Cyclic reference detected in genre hierarchy.");
    }

    [Fact]
    public async Task Should_HaveError_When_ParentIdAndIdMakeCyclicReference()
    {
        var existingGenre = await SeedGenreWithSubGenresAsync();
        var parentGenre = existingGenre.SubGenres.FirstOrDefault();

        var dto = new GenreUpdateDto
        {
            Id = new EntityId(existingGenre.Id),
            ParentGenreId = new EntityId(parentGenre.Id),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.ParentGenreId)
            .WithErrorMessage("Cyclic reference detected in genre hierarchy.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_ParentIdIsNull()
    {
        var dto = new GenreUpdateDto
        {
            Id = new EntityId(Guid.NewGuid()),
            ParentGenreId = null,
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.ParentGenreId);
    }

    private async Task<Genre> SeedGenreAsync()
    {
        var genre = GenreTestData.GetGenre();
        await _genreRepository.AddAsync(genre);
        await _unitOfWork.SaveChangesAsync();
        return genre;
    }

    private async Task<Genre> SeedGenreWithSubGenresAsync()
    {
        var genre = GenreTestData.GetGenreWithSubGenres();
        await _genreRepository.AddAsync(genre);
        await _unitOfWork.SaveChangesAsync();
        return genre;
    }
}