using FluentValidation;
using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Users.Roles;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData.Users;
using Gamestore.BLL.Validators.Users.Roles;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Users;
using Moq;

namespace Gamestore.BLL.Tests.Validators.Users.Roles;

public class RoleCreateDtoValidatorTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Role> _roleRepository;
    private readonly Mock<IValidator<RoleCreateUpdateDto>> _mockBaseValidator;
    private readonly RoleCreateDtoValidator _validator;

    public RoleCreateDtoValidatorTests()
    {
        _unitOfWork = UnitOfWorkFactory.Create();
        _roleRepository = _unitOfWork.Repositories.GetGeneric<Role>();
        _mockBaseValidator = new Mock<IValidator<RoleCreateUpdateDto>>();
        _validator = new RoleCreateDtoValidator(
            _roleRepository,
            _mockBaseValidator.Object);
    }

    [Fact]
    public async Task Should_CallBaseValidator()
    {
        var dto = new RoleCreateDto { Name = "TestRole" };

        await _validator.ValidateAsync(dto);

        _mockBaseValidator.VerifyValidateCalledOnce(dto);
    }

    [Fact]
    public async Task Should_HaveError_When_NameIsNotUnique()
    {
        var existingRole = await SeedRoleAsync();

        var dto = new RoleCreateDto { Name = existingRole.Name! };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Role with this name already exists.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_NameIsUnique()
    {
        var dto = new RoleCreateDto { Name = "NewUniqueRoleName" };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    private async Task<Role> SeedRoleAsync()
    {
        var role = RoleTestData.GetRole();
        await _roleRepository.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();
        return role;
    }
}