using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Auth;
using Gamestore.BLL.Validators.Auth;

namespace Gamestore.BLL.Tests.Validators.Auth;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator;

    public LoginRequestValidatorTests()
    {
        _validator = new LoginRequestValidator();
    }

    [Fact]
    public async Task Should_HaveError_When_LoginIsEmpty()
    {
        var request = new LoginRequest
        {
            Login = string.Empty,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Login)
            .WithErrorMessage("Login is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_PasswordIsEmpty()
    {
        var request = new LoginRequest
        {
            Password = string.Empty,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_RequestIsValid()
    {
        var request = new LoginRequest
        {
            Login = "validLogin",
            Password = "validPassword",
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}