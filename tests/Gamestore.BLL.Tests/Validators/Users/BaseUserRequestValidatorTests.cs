using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Users;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Tests.TestData.Users;
using Gamestore.BLL.Validators.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Users;

namespace Gamestore.BLL.Tests.Validators.Users;

public class BaseUserRequestValidatorTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Role> _roleRepository;
    private readonly BaseUserRequestValidator _validator;

    public BaseUserRequestValidatorTests()
    {
        _unitOfWork = UnitOfWorkFactory.Create();
        _roleRepository = _unitOfWork.Repositories.GetGeneric<Role>();
        _validator = new BaseUserRequestValidator(_roleRepository);
    }

    [Fact]
    public async Task Should_HaveError_When_EmptyRoles()
    {
        var request = new CreateUserRequest
        {
            Roles = new List<Guid>(),
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Roles)
            .WithErrorMessage("At least one role is required.");
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task Should_HaveError_When_RoleIdIsInvalid(Guid invalidId)
    {
        var request = new CreateUserRequest
        {
            Roles = new List<Guid> { invalidId },
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor("Roles[0]")
            .WithErrorMessage("Role Id is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_RoleIdDoesNotExist()
    {
        var roleId = Guid.NewGuid();
        var request = new CreateUserRequest
        {
            Roles = new List<Guid> { roleId },
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor("Roles[0]")
            .WithErrorMessage($"Role with Id {roleId} does not exist.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_RolesAreValid()
    {
        var role = await SeedRoleAsync();
        var request = new CreateUserRequest
        {
            Roles = new List<Guid> { role.Id },
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Roles);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task Should_HaveError_When_EmptyPassword(string invalidPassword)
    {
        var request = new CreateUserRequest
        {
            Password = invalidPassword,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_PasswordIsTooShort()
    {
        var request = new CreateUserRequest
        {
            Password = new string('a', UserValidationRules.MinPasswordLength - 1),
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage($"Password must be at least {UserValidationRules.MinPasswordLength} characters long.");
    }

    [Fact]
    public async Task Should_HaveError_When_PasswordIsTooLong()
    {
        var request = new CreateUserRequest
        {
            Password = new string('a', UserValidationRules.MaxPasswordLength + 1),
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage($"Password must not exceed {UserValidationRules.MaxPasswordLength} characters.");
    }

    [Fact]
    public async Task Should_HaveError_When_PasswordDoesNotContainUppercaseLetter()
    {
        var request = new CreateUserRequest
        {
            Password = "password1!",
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public async Task Should_HaveError_When_PasswordDoesNotContainLowercaseLetter()
    {
        var request = new CreateUserRequest
        {
            Password = "PASSWORD1!",
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one lowercase letter.");
    }

    [Fact]
    public async Task Should_HaveError_When_PasswordDoesNotContainDigit()
    {
        var request = new CreateUserRequest
        {
            Password = "Password!",
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one number.");
    }

    [Fact]
    public async Task Should_HaveError_When_PasswordDoesNotContainSpecialCharacter()
    {
        var request = new CreateUserRequest
        {
            Password = "Password1",
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one special character.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_PasswordIsValid()
    {
        var request = new CreateUserRequest
        {
            Password = "StrongP@ssw0rd",
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    private async Task<Role> SeedRoleAsync()
    {
        var role = RoleTestData.GetRole();
        await _roleRepository.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();
        return role;
    }
}