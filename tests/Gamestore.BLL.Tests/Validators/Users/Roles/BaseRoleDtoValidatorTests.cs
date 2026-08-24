using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Users.Roles;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Validators.Users.Roles;

namespace Gamestore.BLL.Tests.Validators.Users.Roles;

public class BaseRoleDtoValidatorTests
{
    private readonly BaseRoleDtoValidator _validator;

    public BaseRoleDtoValidatorTests()
    {
        _validator = new BaseRoleDtoValidator();
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task Should_HaveError_When_NameIsEmpty(string invalidName)
    {
        var dto = new RoleCreateDto { Name = invalidName };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_NameIsTooShort()
    {
        var dto = new RoleCreateDto
        {
            Name = new string('a', RoleValidationRules.MinNameLength - 1),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage($"Name must be at least {RoleValidationRules.MinNameLength} characters long.");
    }

    [Fact]
    public async Task Should_HaveError_When_NameIsTooLong()
    {
        var dto = new RoleCreateDto
        {
            Name = new string('a', RoleValidationRules.MaxNameLength + 1),
        };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage($"Name must not exceed {RoleValidationRules.MaxNameLength} characters.");
    }

    [Theory]
    [MemberData(nameof(RoleNamesWithInvalidCharacters))]
    public async Task Should_HaveError_When_NameHasInvalidCharacters(string invalidName)
    {
        var dto = new RoleCreateDto { Name = invalidName };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(RoleValidationRules.NamePatternMessage);
    }

    [Theory]
    [MemberData(nameof(ValidRoleNames))]
    public async Task Should_NotHaveError_When_NameIsValid(string name)
    {
        var dto = new RoleCreateDto { Name = name };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    public static TheoryData<string> RoleNamesWithInvalidCharacters()
    {
        return new TheoryData<string>
        {
            "Admin123",
            "User!",
            "Manager@2024",
            "Guest#Role",
            "Super*User",
            "Dev&Ops",
            "Test(Role)",
            "Lead+Developer",
            "Owner=Main",
            "Contributor/Member",
            "Editor\\Content",
            "Moderator:Chat",
            "Analyst;Data",
            "Designer,UI",
            "Architect-Systems",
            "Planner.Future",
        };
    }

    public static TheoryData<string> ValidRoleNames()
    {
        return new TheoryData<string>
        {
            "Admin",
            "User",
            "Manager Role",
            "Guest User",
            "Super User",
            "Dev Ops",
            "Test Role",
            "Lead Developer",
            "Owner Main",
            "Contributor Member",
            "Editor Content",
            "Moderator Chat",
            "Analyst Data",
            "Designer UI",
            "Architect Systems",
            "Planner Future",
            "A B",
            new('a', RoleValidationRules.MaxNameLength),
        };
    }
}