using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Users;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Validators.Users;

namespace Gamestore.BLL.Tests.Validators.Users;

public class BaseUserDtoValidatorTests
{
    private readonly BaseUserDtoValidator _validator;

    public BaseUserDtoValidatorTests()
    {
        _validator = new BaseUserDtoValidator();
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task Should_HaveError_When_NameIsEmpty(string invalidName)
    {
        var dto = new UserCreateDto { Name = invalidName };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Username is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_NameIsTooShort()
    {
        var dto = new UserCreateDto
        {
            Name = new string('a', UserValidationRules.MinUserNameLength - 1),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage($"Username must be at least {UserValidationRules.MinUserNameLength} characters long.");
    }

    [Fact]
    public async Task Should_HaveError_When_NameIsTooLong()
    {
        var dto = new UserCreateDto
        {
            Name = new string('a', UserValidationRules.MaxUserNameLength + 1),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage($"Username must not exceed {UserValidationRules.MaxUserNameLength} characters.");
    }

    [Theory]
    [MemberData(nameof(UserNamesWithInvalidCharacters))]
    public async Task Should_HaveError_When_NameHasInvalidCharacters(string invalidName)
    {
        var dto = new UserCreateDto { Name = invalidName };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(UserValidationRules.UserNamePatternMessage);
    }

    [Theory]
    [MemberData(nameof(ValidUserNames))]
    public async Task Should_NotHaveError_When_NameIsValid(string name)
    {
        var dto = new UserCreateDto { Name = name };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    public static TheoryData<string> UserNamesWithInvalidCharacters()
    {
        return new TheoryData<string>
        {
            "John Doe",
            "Jane# Doe",
            "Alice#1",
            "Bob!",
            "Tom&Jerry",
            "Mary*Jane",
            "Peter%Parker",
            "Bruce^Wayne",
            "Clark<Superman>",
            "Tony|Stark",
            "Steve\\Rogers",
            "Hulk$",
            "Loki?",
            "Thor~Odin",
        };
    }

    public static TheoryData<string> ValidUserNames()
    {
        return new TheoryData<string>
        {
            "JohnDoe",
            "Jane123",
            "User_Name",
            "Alice_In_Wonderland",
            "Bob_the_Builder_2024",
            "Z99",
            "Mega.Ultra.Super.User",
            "Player1",
            "Team-Rocket-1999",
            "ABC",
            new('a', UserValidationRules.MaxUserNameLength),
        };
    }
}