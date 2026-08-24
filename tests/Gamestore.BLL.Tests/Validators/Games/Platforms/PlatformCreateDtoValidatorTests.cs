using FluentValidation;
using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Games.Platforms;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.BLL.Validators.Games.Platforms;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;
using Moq;

namespace Gamestore.BLL.Tests.Validators.Games.Platforms;

public class PlatformCreateDtoValidatorTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Platform> _platformRepository;
    private readonly Mock<IValidator<PlatformCreateUpdateDto>> _mockBaseValidator;
    private readonly PlatformCreateDtoValidator _validator;

    public PlatformCreateDtoValidatorTests()
    {
        _unitOfWork = UnitOfWorkFactory.Create();
        _platformRepository = _unitOfWork.Repositories.GetGeneric<Platform>();
        _mockBaseValidator = new Mock<IValidator<PlatformCreateUpdateDto>>();
        _validator = new PlatformCreateDtoValidator(
            _platformRepository,
            _mockBaseValidator.Object);
    }

    [Fact]
    public async Task Should_CallBaseValidator()
    {
        var dto = new PlatformCreateDto { Type = "Test Platform" };

        await _validator.ValidateAsync(dto);

        _mockBaseValidator.VerifyValidateCalledOnce(dto);
    }

    [Fact]
    public async Task Should_HaveError_When_TypeIsNotUnique()
    {
        var existingPlatform = await SeedPlatformAsync();

        var dto = new PlatformCreateDto { Type = existingPlatform.Type };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("Platform with this type already exists.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_TypeIsUnique()
    {
        var dto = new PlatformCreateDto { Type = "New Platform" };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    private async Task<Platform> SeedPlatformAsync()
    {
        var platform = PlatformTestData.GetPlatform();
        await _platformRepository.AddAsync(platform);
        await _unitOfWork.SaveChangesAsync();
        return platform;
    }
}