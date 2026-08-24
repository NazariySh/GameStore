using FluentValidation;
using Gamestore.BLL.DTOs.Users;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Logging;
using Gamestore.BLL.Services;
using Gamestore.BLL.Services.Users;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Tests.TestData.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Entities.Users;
using Gamestore.Domain.Exceptions;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Services.Users;

public class UserServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IMapper _mapper;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        var mockUserContext = new Mock<IUserContext>();

        _unitOfWork = UnitOfWorkFactory.Create(mockUserContext.Object);
        _userRepository = _unitOfWork.Repositories.GetGeneric<User>();
        _roleRepository = _unitOfWork.Repositories.GetGeneric<Role>();

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(uow => uow.Repositories.GetGeneric<User>())
            .Returns(_userRepository);
        mockUnitOfWork.Setup(uow => uow.Repositories.GetGeneric<Role>())
            .Returns(_roleRepository);

        var transaction = new Mock<IDbContextTransaction>();
        mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);

        _mapper = MapperFactory.Create();
        _mockValidationService = new Mock<IValidationService>();
        _mockUserManager = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        var mockEntityChangeLogService = new Mock<IEntityChangeLogService>();

        _userService = new UserService(
            new ServiceContext(mockUnitOfWork.Object, _mapper, _mockValidationService.Object, mockUserContext.Object),
            _mockUserManager.Object,
            mockEntityChangeLogService.Object,
            Mock.Of<ILogger<UserService>>());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnUsers_WhenUsersExist()
    {
        var users = await SeedUsersAsync();

        var userDtos = _mapper.Map<List<UserDto>>(users);

        var result = await _userService.GetAllAsync();

        Assert.NotEmpty(result);
        Assert.Equivalent(userDtos, result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        var result = await _userService.GetAllAsync();

        Assert.Empty(result);
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task GetByIdAsync_ShouldThrowArgumentException_WhenIdIsInvalid(Guid invalidId)
    {
        var act = () => _userService.GetByIdAsync(invalidId);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();

        var act = () => _userService.GetByIdAsync(userId);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenIdIsValid()
    {
        var user = await SeedUserAsync();

        var userDto = _mapper.Map<UserDto>(user);

        var result = await _userService.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equivalent(userDto, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var request = UserTestData.GetInvalidCreateUserRequest();

        _mockValidationService.SetupValidationThrows(request);

        var act = () => _userService.CreateAsync(request);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenUserCreationFails()
    {
        var request = UserTestData.GetCreateUserRequest();

        SetupMockUserManagerCreateUser(false);

        var act = () => _userService.CreateAsync(request);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("Failed to create user", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenAddToRolesFails()
    {
        var request = UserTestData.GetCreateUserRequest();

        SetupMockUserManagerCreateUser(true);
        SetupMockUserManagerAddToRoles(false);

        var act = () => _userService.CreateAsync(request);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("Failed to add user to roles", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateUser_WhenRequestIsValid()
    {
        var request = UserTestData.GetCreateUserRequest();

        SetupMockUserManagerCreateUser(true);
        SetupMockUserManagerAddToRoles(true);

        var result = await _userService.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal(request.User.Name, result.UserName);

        _mockUserManager.Verify(um => um.CreateAsync(It.IsAny<User>(), request.Password), Times.Once);
        _mockUserManager.Verify(um => um.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenIdIsInvalid(Guid invalidId)
    {
        var request = UserTestData.GetUpdateUserRequest(invalidId);

        var act = () => _userService.UpdateAsync(request);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        var request = UserTestData.GetUpdateUserRequest();

        var act = () => _userService.UpdateAsync(request);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var user = await SeedUserWithRolesAsync();

        var request = UserTestData.GetInvalidUpdateUserRequest(user.Id);

        _mockValidationService.SetupValidationThrows(request);

        var act = () => _userService.UpdateAsync(request);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenUserUpdateFails()
    {
        var user = await SeedUserWithRolesAsync();

        var request = UserTestData.GetUpdateUserRequest(user.Id);

        SetupMockUserManagerFindById(user.Id, user);
        SetupMockUserManagerUpdateUser(false);

        var act = () => _userService.UpdateAsync(request);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("Failed to update user", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenPasswordResetFails()
    {
        var user = await SeedUserWithRolesAsync();

        var request = UserTestData.GetUpdateUserRequest(user.Id);

        SetupMockUserManagerFindById(user.Id, user);
        SetupMockUserManagerUpdateUser(true);
        SetupMockUserManagerPasswordReset(false);

        var act = () => _userService.UpdateAsync(request);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("Failed to reset password", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser_WhenRequestIsValid()
    {
        var user = await SeedUserWithRolesAsync();

        var request = UserTestData.GetUpdateUserRequest(user.Id);

        SetupMockUserManagerFindById(user.Id, user);
        SetupMockUserManagerUpdateUser(true);
        SetupMockUserManagerPasswordReset(true);
        SetupMockUserManagerGetRoles(new List<string>());
        SetupMockUserManagerRemoveFromRoles(true);
        SetupMockUserManagerAddToRoles(true);

        await _userService.UpdateAsync(request);

        _mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<User>()), Times.Once);
        _mockUserManager.Verify(um => um.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), request.Password), Times.Once);
        _mockUserManager.Verify(um => um.RemoveFromRolesAsync(It.IsAny<User>(), It.IsAny<ICollection<string>>()), Times.Once);
        _mockUserManager.Verify(um => um.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task DeleteAsync_ShouldThrowArgumentException_WhenIdIsInvalid(Guid invalidId)
    {
        var act = () => _userService.DeleteAsync(invalidId);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();

        SetupMockUserManagerFindById(userId, null);

        var act = () => _userService.DeleteAsync(userId);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowInvalidOperationException_WhenUserDeletionFails()
    {
        var user = UserTestData.GetUser();

        SetupMockUserManagerFindById(user.Id, user);
        SetupMockUserManagerDeleteUser(user, false);

        var act = () => _userService.DeleteAsync(user.Id);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("Failed to delete user", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteUser_WhenIdIsValid()
    {
        var user = UserTestData.GetUser();

        SetupMockUserManagerFindById(user.Id, user);
        SetupMockUserManagerDeleteUser(user, true);

        await _userService.DeleteAsync(user.Id);

        _mockUserManager.Verify(um => um.DeleteAsync(user), Times.Once);
    }

    private void SetupMockUserManagerFindById(Guid id, User? user)
    {
        _mockUserManager.Setup(um => um.FindByIdAsync(id.ToString()))
            .ReturnsAsync(user);
    }

    private void SetupMockUserManagerCreateUser(bool result)
    {
        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(result ? IdentityResult.Success : IdentityResult.Failed());
    }

    private void SetupMockUserManagerUpdateUser(bool result)
    {
        _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(result ? IdentityResult.Success : IdentityResult.Failed());
    }

    private void SetupMockUserManagerDeleteUser(User user, bool result)
    {
        _mockUserManager.Setup(um => um.DeleteAsync(user))
            .ReturnsAsync(result ? IdentityResult.Success : IdentityResult.Failed());
    }

    private void SetupMockUserManagerAddToRoles(bool result)
    {
        _mockUserManager.Setup(um => um.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(result ? IdentityResult.Success : IdentityResult.Failed());
    }

    private void SetupMockUserManagerRemoveFromRoles(bool result)
    {
        _mockUserManager.Setup(um => um.RemoveFromRolesAsync(It.IsAny<User>(), It.IsAny<ICollection<string>>()))
            .ReturnsAsync(result ? IdentityResult.Success : IdentityResult.Failed());
    }

    private void SetupMockUserManagerGetRoles(IList<string> roles)
    {
        _mockUserManager.Setup(um => um.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(roles);
    }

    private void SetupMockUserManagerPasswordReset(bool result)
    {
        _mockUserManager.Setup(um => um.GeneratePasswordResetTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("reset-token");

        _mockUserManager.Setup(um => um.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(result ? IdentityResult.Success : IdentityResult.Failed());
    }

    private async Task<User> SeedUserAsync()
    {
        var user = UserTestData.GetUser();
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return user;
    }

    private async Task<List<User>> SeedUsersAsync()
    {
        var users = UserTestData.GetUsers();
        await _userRepository.AddRangeAsync(users);
        await _unitOfWork.SaveChangesAsync();
        return users;
    }

    private async Task<User> SeedUserWithRolesAsync()
    {
        var user = UserTestData.GetUser();
        var userRoles = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var roles = RoleTestData.GetRoles(userRoles);

        await _roleRepository.AddRangeAsync(roles);
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }
}