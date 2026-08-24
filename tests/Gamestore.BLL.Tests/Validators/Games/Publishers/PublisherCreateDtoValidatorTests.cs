using FluentValidation;
using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Games.Publishers;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.BLL.Validators.Games.Publishers;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;
using Moq;

namespace Gamestore.BLL.Tests.Validators.Games.Publishers;

public class PublisherCreateDtoValidatorTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Publisher> _publisherRepository;
    private readonly Mock<IValidator<PublisherCreateUpdateDto>> _mockBaseValidator;
    private readonly PublisherCreateDtoValidator _validator;

    public PublisherCreateDtoValidatorTests()
    {
        _unitOfWork = UnitOfWorkFactory.Create();
        _publisherRepository = _unitOfWork.Repositories.GetGeneric<Publisher>();
        _mockBaseValidator = new Mock<IValidator<PublisherCreateUpdateDto>>();
        _validator = new PublisherCreateDtoValidator(
            _publisherRepository,
            _mockBaseValidator.Object);
    }

    [Fact]
    public async Task Should_CallBaseValidator()
    {
        var dto = new PublisherCreateDto { CompanyName = "Test Publisher" };

        await _validator.ValidateAsync(dto);

        _mockBaseValidator.VerifyValidateCalledOnce(dto);
    }

    [Fact]
    public async Task Should_HaveError_When_CompanyNameIsNotUnique()
    {
        var existingPublisher = await SeedPublisherAsync();

        var dto = new PublisherCreateDto { CompanyName = existingPublisher.CompanyName };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.CompanyName)
            .WithErrorMessage("Publisher with this company name already exists.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_CompanyNameIsUnique()
    {
        var dto = new PublisherCreateDto { CompanyName = "New Publisher" };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.CompanyName);
    }

    private async Task<Publisher> SeedPublisherAsync()
    {
        var publisher = PublisherTestData.GetPublisher();
        await _publisherRepository.AddAsync(publisher);
        await _unitOfWork.SaveChangesAsync();
        return publisher;
    }
}