using FluentValidation;
using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Games.Genres;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.BLL.Validators.Games.Genres;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;
using Moq;

namespace Gamestore.BLL.Tests.Validators.Games.Genres;

public class GenreCreateDtoValidatorTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Genre> _genreRepository;
    private readonly Mock<IValidator<GenreCreateUpdateDto>> _mockBaseValidator;
    private readonly GenreCreateDtoValidator _validator;

    public GenreCreateDtoValidatorTests()
    {
        _unitOfWork = UnitOfWorkFactory.Create();
        _genreRepository = _unitOfWork.Repositories.GetGeneric<Genre>();
        _mockBaseValidator = new Mock<IValidator<GenreCreateUpdateDto>>();
        _validator = new GenreCreateDtoValidator(
            _genreRepository,
            _mockBaseValidator.Object);
    }

    [Fact]
    public async Task Should_CallBaseValidator()
    {
        var dto = new GenreCreateDto { Name = "Test Genre" };

        await _validator.ValidateAsync(dto);

        _mockBaseValidator.VerifyValidateCalledOnce(dto);
    }

    [Fact]
    public async Task Should_HaveError_When_NameIsNotUnique()
    {
        var existingGenre = await SeedGenreAsync();

        var dto = new GenreCreateDto { Name = existingGenre.Name };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Genre with this name already exists.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_NameIsUnique()
    {
        var dto = new GenreCreateDto { Name = "New Genre" };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    private async Task<Genre> SeedGenreAsync()
    {
        var genre = GenreTestData.GetGenre();
        await _genreRepository.AddAsync(genre);
        await _unitOfWork.SaveChangesAsync();
        return genre;
    }
}