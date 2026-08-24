using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Users;
using Gamestore.BLL.Enums;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData.Users;
using Gamestore.BLL.Validators.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Users;

namespace Gamestore.BLL.Tests.Validators.Users;

public class BanUserRequestValidatorTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<User> _userRepository;
    private readonly BanUserRequestValidator _validator;

    public BanUserRequestValidatorTests()
    {
        _unitOfWork = UnitOfWorkFactory.Create();
        _userRepository = _unitOfWork.Repositories.GetGeneric<User>();
        _validator = new BanUserRequestValidator(_userRepository);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task Should_HaveError_When_UserNameIsEmpty(string invalidUserName)
    {
        var request = new BanUserRequest
        {
            User = invalidUserName,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.User)
            .WithErrorMessage("User name is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_UserNameDoesNotExist()
    {
        var request = new BanUserRequest
        {
            User = "NonExistentUser",
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.User)
            .WithErrorMessage($"User with name '{request.User}' does not exist.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_UserNameIsValid()
    {
        var user = await SeedUserAsync();
        var request = new BanUserRequest
        {
            User = user.UserName!,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.User);
    }

    [Fact]
    public async Task Should_HaveError_When_DurationIsNull()
    {
        var request = new BanUserRequest
        {
            User = "validUser",
            Duration = null!,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Duration)
            .WithErrorMessage("Ban duration is required.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_DurationIsValid()
    {
        var request = new BanUserRequest
        {
            User = "validUser",
            Duration = BanDuration.OneWeek,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Duration);
    }

    [Fact]
    public async Task Should_NotHaveError_When_AllValid()
    {
        var user = await SeedUserAsync();
        var request = new BanUserRequest
        {
            User = user.UserName!,
            Duration = BanDuration.OneWeek,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private async Task<User> SeedUserAsync()
    {
        var user = UserTestData.GetUser();
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return user;
    }
}