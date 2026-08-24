using FluentValidation;
using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.BLL.Validators.Games;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Shared;
using Moq;

namespace Gamestore.BLL.Tests.Validators.Games;

public class GameUpdateDtoValidatorTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Game> _gameRepository;
    private readonly Mock<IValidator<GameCreateUpdateDto>> _mockBaseValidator;
    private readonly Mock<GameKeyValidator> _mockGameKeyValidator;
    private readonly GameUpdateDtoValidator _validator;

    public GameUpdateDtoValidatorTests()
    {
        _unitOfWork = UnitOfWorkFactory.Create();
        _gameRepository = _unitOfWork.Repositories.GetGeneric<Game>();
        _mockBaseValidator = new Mock<IValidator<GameCreateUpdateDto>>();
        _mockGameKeyValidator = new Mock<GameKeyValidator>();
        _validator = new GameUpdateDtoValidator(
            _gameRepository,
            _mockBaseValidator.Object,
            _mockGameKeyValidator.Object);
    }

    [Fact]
    public async Task Should_CallBaseValidator()
    {
        var dto = new GameUpdateDto();

        await _validator.ValidateAsync(dto);

        _mockBaseValidator.VerifyValidateCalledOnce(dto);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task Should_HaveError_When_KeyIsEmpty(string invalidKey)
    {
        var dto = new GameUpdateDto { Key = invalidKey };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Key)
            .WithErrorMessage("Key is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_KeyIsNotUnique()
    {
        var existingGame = await SeedGameAsync();

        var dto = new GameUpdateDto { Id = new EntityId(Guid.NewGuid()), Key = existingGame.Key };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Key)
            .WithErrorMessage("Game with this key already exists.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_KeyIsUnique()
    {
        var dto = new GameUpdateDto { Id = new EntityId(Guid.NewGuid()), Key = "UniqueKey" };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Key);

        VerifyMockKeyValidatorValidateCalled(dto.Key, Times.Once);
    }

    private void VerifyMockKeyValidatorValidateCalled(string? key, Func<Times> times)
    {
        _mockGameKeyValidator.Verify(
            x => x.ValidateAsync(
                It.Is<ValidationContext<string>>(
                    ctx => ctx.InstanceToValidate == key),
                It.IsAny<CancellationToken>()),
            times);
    }

    private async Task<Game> SeedGameAsync()
    {
        var game = GameTestData.GetGame();
        await _gameRepository.AddAsync(game);
        await _unitOfWork.SaveChangesAsync();
        return game;
    }
}