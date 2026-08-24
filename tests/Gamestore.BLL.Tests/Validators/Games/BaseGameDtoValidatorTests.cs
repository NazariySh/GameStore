using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Validators.Games;

namespace Gamestore.BLL.Tests.Validators.Games;

public class BaseGameDtoValidatorTests
{
    private readonly BaseGameDtoValidator _validator;

    public BaseGameDtoValidatorTests()
    {
        _validator = new BaseGameDtoValidator();
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task Should_HaveError_When_NameIsEmpty(string invalidName)
    {
        var dto = new GameCreateDto { Name = invalidName };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_NameIsTooShort()
    {
        var dto = new GameCreateDto
        {
            Name = new string('a', GameValidationRules.MinNameLength - 1),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage($"Name must be at least {GameValidationRules.MinNameLength} characters long.");
    }

    [Fact]
    public async Task Should_HaveError_When_NameIsTooLong()
    {
        var dto = new GameCreateDto
        {
            Name = new string('a', GameValidationRules.MaxNameLength + 1),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage($"Name must not exceed {GameValidationRules.MaxNameLength} characters.");
    }

    [Theory]
    [MemberData(nameof(GameNamesWithInvalidCharacters))]
    public async Task Should_HaveError_When_NameHasInvalidCharacters(string invalidName)
    {
        var dto = new GameCreateDto { Name = invalidName };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(GameValidationRules.NamePatternMessage);
    }

    [Theory]
    [MemberData(nameof(ValidGameNames))]
    public async Task Should_NotHaveError_When_NameIsValid(string name)
    {
        var dto = new GameCreateDto { Name = name };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Should_HaveError_When_DescriptionIsTooLong()
    {
        var dto = new GameCreateDto
        {
            Description = new string('a', GameValidationRules.MaxDescriptionLength + 1),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage($"Description must not exceed {GameValidationRules.MaxDescriptionLength} characters.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_DescriptionIsValid()
    {
        var dto = new GameCreateDto
        {
            Description = "This is a valid description.",
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Should_NotHaveError_When_DescriptionIsEmpty()
    {
        var dto = new GameCreateDto { Description = string.Empty };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(100_000)]
    public async Task Should_HaveError_When_PriceIsInvalid(decimal invalidPrice)
    {
        var dto = new GameCreateDto { Price = invalidPrice };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage($"Price must be between {GameValidationRules.MinPrice} and {GameValidationRules.MaxPrice}.");
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(49.99)]
    [InlineData(10_000.0)]
    public async Task Should_NotHaveError_When_PriceIsValid(decimal price)
    {
        var dto = new GameCreateDto { Price = price };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public async Task Should_HaveError_When_UnitInStockIsNegativeNumber()
    {
        var dto = new GameCreateDto { UnitInStock = -1 };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.UnitInStock)
            .WithErrorMessage("Unit in stock cannot be negative.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_UnitInStockIsValid()
    {
        var dto = new GameCreateDto { UnitInStock = 20 };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.UnitInStock);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task Should_HaveError_When_DiscountIsInvalid(int invalidDiscount)
    {
        var dto = new GameCreateDto { Discount = invalidDiscount };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Discount)
            .WithErrorMessage($"Discount must be between {GameValidationRules.MinDiscount} and {GameValidationRules.MaxDiscount}.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task Should_NotHaveError_When_DiscountIsValid(int discount)
    {
        var dto = new GameCreateDto { Discount = discount };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Discount);
    }

    [Fact]
    public async Task Should_NotHaveError_When_AllValid()
    {
        var dto = new GameCreateDto
        {
            Name = "Valid Game Name",
            Description = "This is a valid description.",
            Price = 49.99m,
            UnitInStock = 10,
            Discount = 20,
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    public static TheoryData<string> GameNamesWithInvalidCharacters()
    {
        return new TheoryData<string>
        {
            "Call of Duty#Elite",
            "Final Fantasy!",
            "RPG@Night",
            "Shooter $quad",
            "Tales%Rewritten",
            "Kingdom^Fall",
            "Dark&Light",
            "Action*Hero",
            "The Game(Name)",
            "Quest<>Zone",
            "Legends|Rise",
            "The End~Game",
            "Fast=Furious",
            "Racing/Simulator",
            "Horror\\Night",
            "Survive+Now",
            "FPS_Elite!",
        };
    }

    public static TheoryData<string> ValidGameNames()
    {
        return new TheoryData<string>
        {
            "The Witcher's Path",
            "Final Fantasy XIV",
            "RPG 2.0",
            "A Hero's Journey",
            "Call of Honor: Modern Duty",
            "Tales of the Forgotten",
            "Cyber Quest 2077",
            "Platformer.Name-Test",
            "Adventure Reborn 3",
            "Space.Odyssey",
            "Kingdom's Rise",
            "Open World RPG",
        };
    }
}