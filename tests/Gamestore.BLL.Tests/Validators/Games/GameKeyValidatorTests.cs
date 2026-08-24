using FluentValidation.TestHelper;
using Gamestore.BLL.Validators.Games;

namespace Gamestore.BLL.Tests.Validators.Games;

public class GameKeyValidatorTests
{
    private readonly GameKeyValidator _validator;

    public GameKeyValidatorTests()
    {
        _validator = new GameKeyValidator();
    }

    [Fact]
    public async Task Should_HaveError_When_KeyIsTooShort()
    {
        var key = new string('a', GameValidationRules.MinKeyLength - 1);

        var result = await _validator.TestValidateAsync(key);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage($"Key must be at least {GameValidationRules.MinKeyLength} characters long.");
    }

    [Fact]
    public async Task Should_HaveError_When_KeyIsTooLong()
    {
        var key = new string('a', GameValidationRules.MaxKeyLength + 1);

        var result = await _validator.TestValidateAsync(key);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage($"Key must not exceed {GameValidationRules.MaxKeyLength} characters.");
    }

    [Theory]
    [MemberData(nameof(KeysWithInvalidCharacters))]
    public async Task Should_HaveError_When_KeyDoesNotMatchPattern(string invalidKey)
    {
        var result = await _validator.TestValidateAsync(invalidKey);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage(GameValidationRules.KeyPatternMessage);
    }

    [Theory]
    [MemberData(nameof(ValidGameKeys))]
    public async Task Should_NotHaveError_When_KeyIsValid(string key)
    {
        var result = await _validator.TestValidateAsync(key);

        result.ShouldNotHaveAnyValidationErrors();
    }

    public static TheoryData<string> KeysWithInvalidCharacters()
    {
        return new TheoryData<string>
        {
            "INVALID",
            "key_with_underscores",
            "key!@#",
            "key with spaces",
            "key.with.dots",
            "key/with/slash",
            "key+plus",
            "key$money",
            "Key-With-Caps",
        };
    }

    public static TheoryData<string> ValidGameKeys()
    {
        return new TheoryData<string>
        {
            "valid-key",
            "abc123",
            "a-b-c-1-2-3",
            "final-fantasy-xiv",
            "super-mario-bros-3",
            "call-of-duty",
            "rpg-the-awakening",
            "space-odyssey",
        };
    }
}