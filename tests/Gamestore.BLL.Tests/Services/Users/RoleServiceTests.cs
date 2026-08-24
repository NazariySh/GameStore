using System.Security.Claims;
using FluentValidation;
using Gamestore.BLL.DTOs.Users.Roles;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Logging;
using Gamestore.BLL.Services;
using Gamestore.BLL.Services.Users;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Tests.TestData.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Repositories.Interfaces.Users;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Constants;
using Gamestore.Domain.Entities.Users;
using Gamestore.Domain.Exceptions;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Services.Users;

public class RoleServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRoleRepository _roleRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IMapper _mapper;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<RoleManager<Role>> _mockRoleManager;
    private readonly RoleService _roleService;

    public RoleServiceTests()
    {
        var mockUserContext = new Mock<IUserContext>();

        _unitOfWork = UnitOfWorkFactory.Create(mockUserContext.Object);
        _roleRepository = _unitOfWork.Repositories.Get<IRoleRepository>();
        _userRepository = _unitOfWork.Repositories.GetGeneric<User>();

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(uow => uow.Repositories.Get<IRoleRepository>())
            .Returns(_roleRepository);

        var transaction = new Mock<IDbContextTransaction>();
        mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);

        _mapper = MapperFactory.Create();
        _mockValidationService = new Mock<IValidationService>();
        _mockRoleManager = new Mock<RoleManager<Role>>(Mock.Of<IRoleStore<Role>>(), null, null, null, null);

        var mockEntityChangeLogService = new Mock<IEntityChangeLogService>();

        _roleService = new RoleService(
            new ServiceContext(mockUnitOfWork.Object, _mapper, _mockValidationService.Object, mockUserContext.Object),
            _mockRoleManager.Object,
            mockEntityChangeLogService.Object,
            Mock.Of<ILogger<RoleService>>());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnRoles_WhenRolesExist()
    {
        var roles = await SeedRolesAsync();

        var roleDtos = _mapper.Map<List<RoleDto>>(roles);

        var result = await _roleService.GetAllAsync();

        Assert.NotEmpty(result);
        Assert.Equivalent(roleDtos, result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoRolesExist()
    {
        var result = await _roleService.GetAllAsync();

        Assert.Empty(result);
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task GetUserRolesAsync_ShouldThrowArgumentException_WhenUserIdIsInvalid(Guid invalidUserId)
    {
        var act = () => _roleService.GetUserRolesAsync(invalidUserId);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetUserRolesAsync_ShouldReturnUserRoles_WhenUserHasRoles()
    {
        var (user, roles) = await SeedUserWithRolesAsync();

        var roleDtos = _mapper.Map<List<RoleDto>>(roles);

        var result = await _roleService.GetUserRolesAsync(user.Id);

        Assert.NotEmpty(result);
        Assert.Equivalent(roleDtos, result);
    }

    [Fact]
    public async Task GetUserRolesAsync_ShouldReturnEmptyList_WhenUserHasNoRoles()
    {
        var userId = Guid.NewGuid();

        var result = await _roleService.GetUserRolesAsync(userId);

        Assert.Empty(result);
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task GetByIdAsync_ShouldThrowArgumentException_WhenIdIsInvalid(Guid invalidRoleId)
    {
        var act = () => _roleService.GetByIdAsync(invalidRoleId);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenRoleDoesNotExist()
    {
        var roleId = Guid.NewGuid();

        var act = () => _roleService.GetByIdAsync(roleId);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnRole_WhenIdIsValid()
    {
        var role = await SeedRoleAsync();

        var roleDto = _mapper.Map<RoleDto>(role);

        var result = await _roleService.GetByIdAsync(role.Id);

        Assert.NotNull(result);
        Assert.Equivalent(roleDto, result);
    }

    [Fact]
    public async Task GetAllPermissionsAsync_ShouldReturnAllPermissions_WhenPermissionsExist()
    {
        var roles = await SeedRolesAsync();

        var allPermissions = roles
            .SelectMany(r => r.RoleClaims)
            .Where(rc => rc.ClaimType == CustomClaimTypes.Permission)
            .Select(rc => rc.ClaimValue)
            .Distinct()
            .ToList();

        var result = await _roleService.GetAllPermissionsAsync();

        Assert.NotEmpty(result);
        Assert.Equivalent(allPermissions, result);
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task GetRolePermissionsAsync_ShouldThrowArgumentException_WhenRoleIdIsInvalid(Guid invalidRoleId)
    {
        var act = () => _roleService.GetRolePermissionsAsync(invalidRoleId);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetRolePermissionsAsync_ShouldReturnPermissions_WhenRoleIdIsValid()
    {
        var role = await SeedRoleAsync();

        var rolePermissions = role.RoleClaims
            .Where(rc => rc.ClaimType == CustomClaimTypes.Permission)
            .Select(rc => rc.ClaimValue)
            .ToList();

        var result = await _roleService.GetRolePermissionsAsync(role.Id);

        Assert.NotEmpty(result);
        Assert.Equivalent(rolePermissions, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var request = RoleTestData.GetInvalidCreateRequest();

        _mockValidationService.SetupValidationThrows(request);

        var act = () => _roleService.CreateAsync(request);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenRoleCreationFails()
    {
        var request = RoleTestData.GetCreateRequest();

        SetupMockRoleManagerCreateRole(false);

        var act = () => _roleService.CreateAsync(request);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("Failed to create role", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenAddPermissionFails()
    {
        var request = RoleTestData.GetCreateRequest();

        SetupMockRoleManagerCreateRole(true);
        SetupMockRoleManagerAddClaim(false);

        var act = () => _roleService.CreateAsync(request);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("Failed to add permission", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateRole_WhenRequestIsValid()
    {
        var request = RoleTestData.GetCreateRequest();

        SetupMockRoleManagerCreateRole(true);
        SetupMockRoleManagerAddClaim(true);

        var result = await _roleService.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal(request.Role.Name, result.Name);

        _mockRoleManager.Verify(rm => rm.CreateAsync(It.IsAny<Role>()), Times.Once);
        _mockRoleManager.Verify(rm => rm.AddClaimAsync(It.IsAny<Role>(), It.IsAny<Claim>()), Times.Exactly(request.Permissions.Count));
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenIdIsInvalid(Guid invalidId)
    {
        var request = RoleTestData.GetUpdateRequest(invalidId);

        var act = () => _roleService.UpdateAsync(request);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenRoleDoesNotExist()
    {
        var request = RoleTestData.GetUpdateRequest();

        var act = () => _roleService.UpdateAsync(request);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var role = await SeedRoleAsync();

        var request = RoleTestData.GetInvalidUpdateRequest(role.Id);

        _mockValidationService.SetupValidationThrows(request);

        var act = () => _roleService.UpdateAsync(request);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenRoleUpdateFails()
    {
        var role = await SeedRoleAsync();

        var request = RoleTestData.GetUpdateRequest(role.Id);

        SetupMockRoleManagerFindById(role.Id, role);
        SetupMockRoleManagerUpdateRole(false);

        var act = () => _roleService.UpdateAsync(request);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("Failed to update role", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenRemovePermissionFails()
    {
        var role = await SeedRoleAsync();
        var request = RoleTestData.GetUpdateRequest(role.Id);

        SetupMockRoleManagerFindById(role.Id, role);
        SetupMockRoleManagerUpdateRole(true);
        SetupMockRoleManagerGetClaims(role.RoleClaims);
        SetupMockRoleManagerRemoveClaim(false);

        var act = () => _roleService.UpdateAsync(request);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("Failed to remove permission", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenAddPermissionFails()
    {
        var role = await SeedRoleAsync();
        var request = RoleTestData.GetUpdateRequest(role.Id);

        SetupMockRoleManagerFindById(role.Id, role);
        SetupMockRoleManagerUpdateRole(true);
        SetupMockRoleManagerGetClaims(role.RoleClaims);
        SetupMockRoleManagerRemoveClaim(true);
        SetupMockRoleManagerAddClaim(false);

        var act = () => _roleService.UpdateAsync(request);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("Failed to add permission", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateRole_WhenRequestIsValid()
    {
        var role = await SeedRoleAsync();
        var request = RoleTestData.GetUpdateRequest(role.Id);

        SetupMockRoleManagerFindById(role.Id, role);
        SetupMockRoleManagerUpdateRole(true);
        SetupMockRoleManagerGetClaims(role.RoleClaims);
        SetupMockRoleManagerRemoveClaim(true);
        SetupMockRoleManagerAddClaim(true);

        await _roleService.UpdateAsync(request);

        _mockRoleManager.Verify(rm => rm.UpdateAsync(It.IsAny<Role>()), Times.Once);
        _mockRoleManager.Verify(rm => rm.RemoveClaimAsync(It.IsAny<Role>(), It.IsAny<Claim>()), Times.Exactly(role.RoleClaims.Count));
        _mockRoleManager.Verify(rm => rm.AddClaimAsync(It.IsAny<Role>(), It.IsAny<Claim>()), Times.Exactly(request.Permissions.Count));
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task DeleteAsync_ShouldThrowArgumentException_WhenIdIsInvalid(Guid invalidId)
    {
        var act = () => _roleService.DeleteAsync(invalidId);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenRoleDoesNotExist()
    {
        var roleId = Guid.NewGuid();

        SetupMockRoleManagerFindById(roleId, null);

        var act = () => _roleService.DeleteAsync(roleId);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowInvalidOperationException_WhenRoleDeletionFails()
    {
        var role = RoleTestData.GetRole();

        SetupMockRoleManagerFindById(role.Id, role);
        SetupMockRoleManagerDeleteRole(role, false);

        var act = () => _roleService.DeleteAsync(role.Id);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("Failed to delete role", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteRole_WhenIdIsValid()
    {
        var role = RoleTestData.GetRole();

        SetupMockRoleManagerFindById(role.Id, role);
        SetupMockRoleManagerDeleteRole(role, true);

        await _roleService.DeleteAsync(role.Id);

        _mockRoleManager.Verify(rm => rm.DeleteAsync(role), Times.Once);
    }

    private void SetupMockRoleManagerFindById(Guid id, Role role)
    {
        _mockRoleManager.Setup(rm => rm.FindByIdAsync(id.ToString()))
            .ReturnsAsync(role);
    }

    private void SetupMockRoleManagerCreateRole(bool result)
    {
        _mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<Role>()))
            .ReturnsAsync(result ? IdentityResult.Success : IdentityResult.Failed());
    }

    private void SetupMockRoleManagerUpdateRole(bool result)
    {
        _mockRoleManager.Setup(rm => rm.UpdateAsync(It.IsAny<Role>()))
            .ReturnsAsync(result ? IdentityResult.Success : IdentityResult.Failed());
    }

    private void SetupMockRoleManagerDeleteRole(Role role, bool result)
    {
        _mockRoleManager.Setup(rm => rm.DeleteAsync(role))
            .ReturnsAsync(result ? IdentityResult.Success : IdentityResult.Failed());
    }

    private void SetupMockRoleManagerAddClaim(bool result)
    {
        _mockRoleManager.Setup(rm => rm.AddClaimAsync(It.IsAny<Role>(), It.IsAny<Claim>()))
            .ReturnsAsync(result ? IdentityResult.Success : IdentityResult.Failed());
    }

    private void SetupMockRoleManagerRemoveClaim(bool result)
    {
        _mockRoleManager.Setup(rm => rm.RemoveClaimAsync(It.IsAny<Role>(), It.IsAny<Claim>()))
            .ReturnsAsync(result ? IdentityResult.Success : IdentityResult.Failed());
    }

    private void SetupMockRoleManagerGetClaims(ICollection<RoleClaim> roleClaims)
    {
        var claims = roleClaims
            .Select(rc => new Claim(rc.ClaimType!, rc.ClaimValue!))
            .ToList();

        _mockRoleManager.Setup(rm => rm.GetClaimsAsync(It.IsAny<Role>()))
            .ReturnsAsync(claims);
    }

    private async Task<Role> SeedRoleAsync()
    {
        var role = RoleTestData.GetRole();
        await _roleRepository.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();
        return role;
    }

    private async Task<List<Role>> SeedRolesAsync()
    {
        var roles = RoleTestData.GetRoles();
        await _roleRepository.AddRangeAsync(roles);
        await _unitOfWork.SaveChangesAsync();
        return roles;
    }

    private async Task<(User User, List<Role> Roles)> SeedUserWithRolesAsync()
    {
        var user = UserTestData.GetUser();
        var userRoles = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var roles = RoleTestData.GetRoles(userRoles);

        await _roleRepository.AddRangeAsync(roles);
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return (user, roles);
    }
}