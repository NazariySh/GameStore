using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Games.Platforms;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Validators.Games.Platforms;

namespace Gamestore.BLL.Tests.Validators.Games.Platforms;

public class BasePlatformDtoValidatorTests
{
    private readonly BasePlatformDtoValidator _validator;

    public BasePlatformDtoValidatorTests()
    {
        _validator = new BasePlatformDtoValidator();
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task Should_HaveError_When_TypeIsEmpty(string invalidType)
    {
        var dto = new PlatformCreateDto { Type = invalidType };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("Type is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_TypeIsTooShort()
    {
        var dto = new PlatformCreateDto
        {
            Type = new string('a', PlatformValidationRules.MinTypeLength - 1),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage($"Type must be at least {PlatformValidationRules.MinTypeLength} characters long.");
    }

    [Fact]
    public async Task Should_HaveError_When_TypeIsTooLong()
    {
        var dto = new PlatformCreateDto
        {
            Type = new string('a', PlatformValidationRules.MaxTypeLength + 1),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage($"Type must not exceed {PlatformValidationRules.MaxTypeLength} characters.");
    }

    [Theory]
    [MemberData(nameof(PlatformTypesWithInvalidCharacters))]
    public async Task Should_HaveError_When_TypeHasInvalidCharacters(string invalidType)
    {
        var dto = new PlatformCreateDto { Type = invalidType };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage(PlatformValidationRules.TypePatternMessage);
    }

    [Theory]
    [MemberData(nameof(ValidPlatformTypes))]
    public async Task Should_NotHaveError_When_TypeIsValid(string type)
    {
        var dto = new PlatformCreateDto { Type = type };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    public static TheoryData<string> PlatformTypesWithInvalidCharacters()
    {
        return new TheoryData<string>
        {
            "PlayStation@5",
            "Xbox#Series",
            "Nintendo%Switch",
            "PC&Gaming",
            "Mobile*Platform",
            "Next !Gen",
            "Retro^Console",
            "Handheld(Type)",
            "Cloud_Platform",
            "Virtual+Reality",
            "Augmented~Reality",
            "Cross/Platform",
            "Multi\\Device",
            "Hybrid=Console",
            "Smart{TV}",
        };
    }

    public static TheoryData<string> ValidPlatformTypes()
    {
        return new TheoryData<string>
        {
            "Game Platform",
            "Platform-Type",
            "Platform 2.0",
            "Platform's Type",
            "Next-Gen Platform",
            "PC",
            "PlayStation 5",
            "Xbox Series X",
            "Nintendo Switch",
            "Mobile",
        };
    }
}