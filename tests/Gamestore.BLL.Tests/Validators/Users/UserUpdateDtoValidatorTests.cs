using FluentValidation;
using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Users;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData.Users;
using Gamestore.BLL.Validators.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Users;
using Moq;

namespace Gamestore.BLL.Tests.Validators.Users;

public class UserUpdateDtoValidatorTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<User> _userRepository;
    private readonly Mock<IValidator<UserCreateUpdateDto>> _mockBaseValidator;
    private readonly UserUpdateDtoValidator _validator;

    public UserUpdateDtoValidatorTests()
    {
        _unitOfWork = UnitOfWorkFactory.Create();
        _userRepository = _unitOfWork.Repositories.GetGeneric<User>();
        _mockBaseValidator = new Mock<IValidator<UserCreateUpdateDto>>();
        _validator = new UserUpdateDtoValidator(
            _userRepository,
            _mockBaseValidator.Object);
    }

    [Fact]
    public async Task Should_CallBaseValidator()
    {
        var dto = new UserUpdateDto { Id = Guid.NewGuid(), Name = "TestUser" };

        await _validator.ValidateAsync(dto);

        _mockBaseValidator.VerifyValidateCalledOnce(dto);
    }

    [Fact]
    public async Task Should_HaveError_When_NameIsNotUnique()
    {
        var existingUser = await SeedUserAsync();

        var dto = new UserUpdateDto { Id = Guid.NewGuid(), Name = existingUser.UserName! };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("User with this name already exists.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_NameIsUnique()
    {
        var dto = new UserUpdateDto { Id = Guid.NewGuid(), Name = "NewUniqueUserName" };

        var result = await _validator.TestValidateAsync(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    private async Task<User> SeedUserAsync()
    {
        var user = UserTestData.GetUser();
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return user;
    }
}