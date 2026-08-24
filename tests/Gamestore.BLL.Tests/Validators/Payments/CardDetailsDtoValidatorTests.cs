using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Payments;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Validators.Payments;

namespace Gamestore.BLL.Tests.Validators.Payments;

public class CardDetailsDtoValidatorTests
{
    private readonly CardDetailsDtoValidator _validator;

    public CardDetailsDtoValidatorTests()
    {
        _validator = new CardDetailsDtoValidator();
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task Should_HaveError_When_HolderIsEmpty(string invalidHolder)
    {
        var cardDetails = new CardDetailsDto
        {
            Holder = invalidHolder,
        };

        var result = await _validator.TestValidateAsync(cardDetails);

        result.ShouldHaveValidationErrorFor(x => x.Holder)
            .WithErrorMessage("Card holder name is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_HolderIsTooShort()
    {
        var cardDetails = new CardDetailsDto
        {
            Holder = new string('a', CreditCardValidationRules.MinHolderLength - 1),
        };

        var result = await _validator.TestValidateAsync(cardDetails);

        result.ShouldHaveValidationErrorFor(x => x.Holder)
            .WithErrorMessage($"Card holder name must be at least {CreditCardValidationRules.MinHolderLength} characters long.");
    }

    [Fact]
    public async Task Should_HaveError_When_HolderIsTooLong()
    {
        var cardDetails = new CardDetailsDto
        {
            Holder = new string('a', CreditCardValidationRules.MaxHolderLength + 1),
        };

        var result = await _validator.TestValidateAsync(cardDetails);

        result.ShouldHaveValidationErrorFor(x => x.Holder)
            .WithErrorMessage($"Card holder name must not exceed {CreditCardValidationRules.MaxHolderLength} characters.");
    }

    [Theory]
    [MemberData(nameof(HolderNamesWithInvalidCharacters))]
    public async Task Should_HaveError_When_HolderHasInvalidCharacters(string invalidHolder)
    {
        var cardDetails = new CardDetailsDto
        {
            Holder = invalidHolder,
        };

        var result = await _validator.TestValidateAsync(cardDetails);

        result.ShouldHaveValidationErrorFor(x => x.Holder)
            .WithErrorMessage(CreditCardValidationRules.HolderPatternMessage);
    }

    [Theory]
    [MemberData(nameof(ValidHolderNames))]
    public async Task Should_NotHaveError_When_HolderIsValid(string holder)
    {
        var cardDetails = new CardDetailsDto
        {
            Holder = holder,
        };

        var result = await _validator.TestValidateAsync(cardDetails);

        result.ShouldNotHaveValidationErrorFor(x => x.Holder);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task Should_HaveError_When_CardNumberIsEmpty(string invalidCardNumber)
    {
        var cardDetails = new CardDetailsDto
        {
            CardNumber = invalidCardNumber,
        };

        var result = await _validator.TestValidateAsync(cardDetails);

        result.ShouldHaveValidationErrorFor(x => x.CardNumber)
            .WithErrorMessage("Card number is required.");
    }

    [Theory]
    [InlineData("41111111")]
    [InlineData("4111-1111-1111")]
    [InlineData("1234-5678-9012-3456")]
    public async Task Should_HaveError_When_CardNumberIsInvalid(string invalidCardNumber)
    {
        var cardDetails = new CardDetailsDto
        {
            CardNumber = invalidCardNumber,
        };

        var result = await _validator.TestValidateAsync(cardDetails);

        result.ShouldHaveValidationErrorFor(x => x.CardNumber)
            .WithErrorMessage("Invalid card number format.");
    }

    [Theory]
    [InlineData("4111111111111111")]
    [InlineData("5500000000000004")]
    public async Task Should_NotHaveError_When_CardNumberIsValid(string cardNumber)
    {
        var cardDetails = new CardDetailsDto
        {
            CardNumber = cardNumber,
        };

        var result = await _validator.TestValidateAsync(cardDetails);

        result.ShouldNotHaveValidationErrorFor(x => x.CardNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public async Task Should_HaveError_When_MonthExpireIsInvalidMonth(int invalidMonth)
    {
        var cardDetails = new CardDetailsDto
        {
            MonthExpire = invalidMonth,
        };

        var result = await _validator.TestValidateAsync(cardDetails);

        result.ShouldHaveValidationErrorFor(x => x.MonthExpire)
            .WithErrorMessage($"Expiration month must be between {CreditCardValidationRules.MinMonth} and {CreditCardValidationRules.MaxMonth}.");
    }

    [Theory]
    [InlineData(5)]
    [InlineData(12)]
    public async Task Should_NotHaveError_When_MonthExpireIsValid(int month)
    {
        var cardDetails = new CardDetailsDto
        {
            MonthExpire = month,
        };

        var result = await _validator.TestValidateAsync(cardDetails);

        result.ShouldNotHaveValidationErrorFor(x => x.MonthExpire);
    }

    [Fact]
    public async Task Should_HaveError_When_YearExpireIsBeforeCurrentYear()
    {
        var cardDetails = new CardDetailsDto
        {
            YearExpire = DateTime.UtcNow.Year - 1,
        };

        var result = await _validator.TestValidateAsync(cardDetails);

        result.ShouldHaveValidationErrorFor(x => x.YearExpire)
            .WithErrorMessage("Expiration year must be in the current year or later.");
    }

    [Fact]
    public async Task Should_HaveError_When_YearExpireIsAfterMaxOffset()
    {
        var cardDetails = new CardDetailsDto
        {
            YearExpire = DateTime.UtcNow.Year + CreditCardValidationRules.MaxYearExpireOffset + 1,
        };

        var result = await _validator.TestValidateAsync(cardDetails);

        result.ShouldHaveValidationErrorFor(x => x.YearExpire)
            .WithErrorMessage($"Expiration year must be within {CreditCardValidationRules.MaxYearExpireOffset} years from now.");
    }

    [Fact]
    public async Task Should_HaveError_When_CardIsExpired()
    {
        var cardDetails = new CardDetailsDto
        {
            MonthExpire = DateTime.UtcNow.Month,
            YearExpire = DateTime.UtcNow.Year - 1,
        };

        var result = await _validator.TestValidateAsync(cardDetails);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("The card is expired.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(000)]
    [InlineData(10000)]
    public async Task Should_HaveError_When_Cvv2IsInvalid(int invalidCvv)
    {
        var cardDetails = new CardDetailsDto
        {
            Cvv2 = invalidCvv,
        };

        var result = await _validator.TestValidateAsync(cardDetails);

        result.ShouldHaveValidationErrorFor(x => x.Cvv2)
            .WithErrorMessage($"CVV must be between {CreditCardValidationRules.MinCvv} and {CreditCardValidationRules.MaxCvv}.");
    }

    [Theory]
    [InlineData(001)]
    [InlineData(090)]
    [InlineData(123)]
    public async Task Should_NotHaveError_When_Cvv2IsValid(int cvv)
    {
        var cardDetails = new CardDetailsDto
        {
            Cvv2 = cvv,
        };

        var result = await _validator.TestValidateAsync(cardDetails);

        result.ShouldNotHaveValidationErrorFor(x => x.Cvv2);
    }

    public static TheoryData<string> HolderNamesWithInvalidCharacters()
    {
        return new TheoryData<string>
        {
            "John Doe!@#",
            "Jane_Doe123",
            "Invalid Holder Name!",
            "1234567890",
            "!@#$%^&*()",
        };
    }

    public static TheoryData<string> ValidHolderNames()
    {
        return new TheoryData<string>
        {
            "John Doe",
            "Jane Smith",
            "Alice Johnson",
            "Bob O'Connor",
            "Charlie Brown",
        };
    }
}