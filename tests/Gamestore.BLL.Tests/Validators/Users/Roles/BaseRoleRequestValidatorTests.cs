using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Users.Roles;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Tests.TestData.Users;
using Gamestore.BLL.Validators.Users.Roles;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Constants;
using Gamestore.Domain.Entities.Users;

namespace Gamestore.BLL.Tests.Validators.Users.Roles;

public class BaseRoleRequestValidatorTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Role> _roleRepository;
    private readonly BaseRoleRequestValidator _validator;

    public BaseRoleRequestValidatorTests()
    {
        _unitOfWork = UnitOfWorkFactory.Create();
        _roleRepository = _unitOfWork.Repositories.GetGeneric<Role>();
        _validator = new BaseRoleRequestValidator(_roleRepository);
    }

    [Fact]
    public async Task Should_HaveError_When_EmptyPermissions()
    {
        var request = new CreateRoleRequest
        {
            Permissions = new List<string>(),
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Permissions)
            .WithErrorMessage("At least one permission is required.");
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task Should_HaveError_When_PermissionIsInvalid(string invalidPermission)
    {
        var request = new CreateRoleRequest
        {
            Permissions = new List<string> { invalidPermission },
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor("Permissions[0]")
            .WithErrorMessage("Permission cannot be empty.");
    }

    [Fact]
    public async Task Should_HaveError_When_PermissionDoesNotExist()
    {
        var invalidPermission = "InvalidPermission";
        var request = new CreateRoleRequest
        {
            Permissions = new List<string> { invalidPermission },
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor("Permissions[0]")
            .WithErrorMessage($"Permission '{invalidPermission}' is not recognized.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_AllValid()
    {
        var role = await SeedRoleWithPermissionsAsync();
        var permissions = role.RoleClaims
            .Where(rc => rc.ClaimType == CustomClaimTypes.Permission)
            .Select(rc => rc.ClaimValue!)
            .ToList();

        var request = new CreateRoleRequest
        {
            Role = new RoleCreateDto
            {
                Name = "NewRole",
            },
            Permissions = permissions,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private async Task<Role> SeedRoleWithPermissionsAsync()
    {
        var role = RoleTestData.GetRole();
        await _roleRepository.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();
        return role;
    }
}