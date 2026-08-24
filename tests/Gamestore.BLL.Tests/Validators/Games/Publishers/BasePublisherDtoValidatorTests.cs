using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Games.Publishers;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Validators.Games.Publishers;

namespace Gamestore.BLL.Tests.Validators.Games.Publishers;

public class BasePublisherDtoValidatorTests
{
    private readonly BasePublisherDtoValidator _validator;

    public BasePublisherDtoValidatorTests()
    {
        _validator = new BasePublisherDtoValidator();
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task Should_HaveError_When_CompanyNameIsEmpty(string invalidCompanyName)
    {
        var dto = new PublisherCreateDto { CompanyName = invalidCompanyName };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.CompanyName)
            .WithErrorMessage("Company Name is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_CompanyNameIsTooShort()
    {
        var dto = new PublisherCreateDto
        {
            CompanyName = new string('a', PublisherValidationRules.MinCompanyNameLength - 1),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.CompanyName)
            .WithErrorMessage($"Company Name must be at least {PublisherValidationRules.MinCompanyNameLength} characters long.");
    }

    [Fact]
    public async Task Should_HaveError_When_CompanyNameIsTooLong()
    {
        var dto = new PublisherCreateDto
        {
            CompanyName = new string('a', PublisherValidationRules.MaxCompanyNameLength + 1),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.CompanyName)
            .WithErrorMessage($"Company Name must not exceed {PublisherValidationRules.MaxCompanyNameLength} characters.");
    }

    [Theory]
    [MemberData(nameof(PublisherCompanyNamesWithInvalidCharacters))]
    public async Task Should_HaveError_When_CompanyNameHasInvalidCharacters(string invalidCompanyName)
    {
        var dto = new PublisherCreateDto { CompanyName = invalidCompanyName };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.CompanyName)
            .WithErrorMessage(PublisherValidationRules.CompanyNamePatternMessage);
    }

    [Theory]
    [MemberData(nameof(ValidPublisherCompanyNames))]
    public async Task Should_NotHaveError_When_CompanyNameIsValid(string companyName)
    {
        var dto = new PublisherCreateDto { CompanyName = companyName };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.CompanyName);
    }

    [Fact]
    public async Task Should_HaveError_When_HomePageIsTooLong()
    {
        var dto = new PublisherCreateDto
        {
            HomePage = new string('a', PublisherValidationRules.MaxHomePageLength + 1),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.HomePage)
            .WithErrorMessage($"Home Page must not exceed {PublisherValidationRules.MaxHomePageLength} characters.");
    }

    [Fact]
    public async Task Should_HaveError_When_HomePageIsInvalidUrl()
    {
        var dto = new PublisherCreateDto { HomePage = "invalid-url" };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.HomePage)
            .WithErrorMessage("Home Page must be a valid URL.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_HomePageIsValidUrl()
    {
        var dto = new PublisherCreateDto { HomePage = "https://example.com" };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.HomePage);
    }

    [Fact]
    public async Task Should_NotHaveError_When_HomePageIsEmpty()
    {
        var dto = new PublisherCreateDto { HomePage = string.Empty };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.HomePage);
    }

    [Fact]
    public async Task Should_HaveError_When_DescriptionIsTooLong()
    {
        var dto = new PublisherCreateDto
        {
            Description = new string('a', PublisherValidationRules.MaxDescriptionLength + 1),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage($"Description must not exceed {PublisherValidationRules.MaxDescriptionLength} characters.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_DescriptionIsValid()
    {
        var dto = new PublisherCreateDto { Description = "A valid description." };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Should_NotHaveError_When_DescriptionIsEmpty()
    {
        var dto = new PublisherCreateDto { Description = string.Empty };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Should_NotHaveError_When_AllValid()
    {
        var dto = new PublisherCreateDto
        {
            CompanyName = "Valid Company",
            HomePage = "https://validcompany.com",
            Description = "A valid description for the publisher.",
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    public static TheoryData<string> PublisherCompanyNamesWithInvalidCharacters()
    {
        return new TheoryData<string>
        {
            "123@",
            "Valve®",
            "Nintendo!",
            "CD Projekt 🚀",
            "Ubisoft*",
            "Rock&Roll Games",
            "Niño$ Games",
            "Jalapeño% Studios",
            "Game<Dev>",
            "CrazyGames()",
        };
    }

    public static TheoryData<string> ValidPublisherCompanyNames()
    {
        return new TheoryData<string>
        {
            "EA",
            "Ubisoft",
            "CD Projekt",
            "Rockstar Games",
            "Larian Studios",
            "FromSoftware",
            "Team Cherry",
            "Devolver Digital",
            "Hello Games",
            "343 Industries",
            "GOG.com",
            "Games-R-Us",
            "O'Reilly Games",
            "Studio-47",
            "Pixel Jump Inc.",
        };
    }
}