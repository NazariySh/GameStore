using System.Security.Claims;
using FluentValidation;
using Gamestore.BLL.DTOs.Auth;
using Gamestore.BLL.Integrations.Interfaces;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Auth;
using Gamestore.BLL.Services;
using Gamestore.BLL.Services.Auth;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.TestData.Auth;
using Gamestore.BLL.Tests.TestData.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Repositories.Interfaces.Users;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Entities.Users;
using Gamestore.Domain.Enums;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Services.Auth;

public class AuthServiceTests
{
    private readonly Mock<IRoleRepository> _mockRoleRepository;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<ITokenProvider> _mockTokenProvider;
    private readonly Mock<IAuthApiClient> _mockAuthApiClient;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRoleRepository = new Mock<IRoleRepository>();
        mockUnitOfWork.Setup(x => x.Repositories.Get<IRoleRepository>())
            .Returns(_mockRoleRepository.Object);

        var mockTransaction = new Mock<IDbContextTransaction>();
        mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        _mockUserManager = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        var mapper = new Mock<IMapper>();
        _mockValidationService = new Mock<IValidationService>();
        var mockUserContext = new Mock<IUserContext>();
        _mockTokenProvider = new Mock<ITokenProvider>();
        _mockAuthApiClient = new Mock<IAuthApiClient>();

        _authService = new AuthService(
            new ServiceContext(mockUnitOfWork.Object, mapper.Object, _mockValidationService.Object, mockUserContext.Object),
            _mockUserManager.Object,
            _mockTokenProvider.Object,
            _mockAuthApiClient.Object,
            Mock.Of<ILogger<AuthService>>());
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var request = AuthTestData.GetInvalidLoginRequest();

        _mockValidationService.SetupValidationThrows(request);

        var act = () => _authService.LoginAsync(request);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowArgumentException_WhenUserNotFound()
    {
        var request = AuthTestData.GetLoginRequest();

        SetupMockUserManagerFindByName(request.Login, null);

        var act = () => _authService.LoginAsync(request);

        var exception = await Assert.ThrowsAsync<ArgumentException>(act);
        Assert.Equal("Invalid login or password", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowArgumentException_WhenPasswordIsInvalid()
    {
        var user = UserTestData.GetUser();
        var request = AuthTestData.GetLoginRequest(user.UserName);

        SetupMockUserManagerFindByName(request.Login, user);
        SetupMockUserManagerCheckPassword(false);

        var act = () => _authService.LoginAsync(request);

        var exception = await Assert.ThrowsAsync<ArgumentException>(act);
        Assert.Equal("Invalid login or password", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnLoginResponse_WhenCredentialsAreValid()
    {
        var user = UserTestData.GetUser();
        var request = AuthTestData.GetLoginRequest(user.UserName);
        var expectedToken = AuthTestData.GetToken();

        SetupMockUserManagerFindByName(request.Login, user);
        SetupMockUserManagerCheckPassword(true);
        SetupIssueUserToken(expectedToken);

        var result = await _authService.LoginAsync(request);

        Assert.NotNull(result);
        Assert.Equal(expectedToken.AccessToken, result.Token);
    }

    [Fact]
    public async Task LoginWithExternalAuthAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var request = AuthTestData.GetInvalidLoginRequest();

        _mockValidationService.SetupValidationThrows(request);

        var act = () => _authService.LoginWithExternalAuthAsync(request);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task LoginWithExternalAuthAsync_ShouldCreateNewUserAndReturnToken_WhenUserDoesNotExist()
    {
        var request = AuthTestData.GetLoginRequest();
        var expectedToken = AuthTestData.GetToken();

        var authResponse = new AuthResponseDto
        {
            Email = request.Login,
            FirstName = "Test",
            LastName = "User",
        };

        SetupMockAuthApiClientLogin(authResponse);
        SetupMockUserManagerFindByEmail(authResponse.Email, null);
        SetupMockUserManagerCreate(true);
        SetupMockUserManagerAddToRole(nameof(RoleType.User), true);
        SetupMockUserManagerAddUserLogin(true);
        SetupIssueUserToken(expectedToken);

        var result = await _authService.LoginWithExternalAuthAsync(request);

        Assert.NotNull(result);
        Assert.Equal(expectedToken.AccessToken, result.Token);
    }

    [Fact]
    public async Task LoginWithExternalAuthAsync_ShouldReturnLoginResponse_WhenUserExists()
    {
        var user = UserTestData.GetUser();
        var request = AuthTestData.GetLoginRequest(user.UserName);
        var expectedToken = AuthTestData.GetToken();

        var authResponse = new AuthResponseDto
        {
            Email = user.UserName!,
            FirstName = "Test",
            LastName = "User",
        };

        SetupMockAuthApiClientLogin(authResponse);
        SetupMockUserManagerFindByEmail(authResponse.Email, user);
        SetupIssueUserToken(expectedToken);

        var result = await _authService.LoginWithExternalAuthAsync(request);

        Assert.NotNull(result);
        Assert.Equal(expectedToken.AccessToken, result.Token);
    }

    private void SetupMockUserManagerFindByName(string login, User? user)
    {
        _mockUserManager.Setup(um => um.FindByNameAsync(login)).ReturnsAsync(user);
    }

    private void SetupMockUserManagerFindByEmail(string email, User? user)
    {
        _mockUserManager.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync(user);
    }

    private void SetupMockUserManagerCheckPassword(bool result)
    {
        _mockUserManager
            .Setup(um => um.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(result);
    }

    private void SetupMockUserManagerCreate(bool result)
    {
        _mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(result ? IdentityResult.Success : IdentityResult.Failed());
    }

    private void SetupMockUserManagerAddToRole(string role, bool result)
    {
        _mockUserManager
            .Setup(um => um.AddToRoleAsync(It.IsAny<User>(), role))
            .ReturnsAsync(result ? IdentityResult.Success : IdentityResult.Failed());
    }

    private void SetupMockUserManagerAddUserLogin(bool result)
    {
        _mockUserManager
            .Setup(um => um.AddLoginAsync(It.IsAny<User>(), It.IsAny<UserLoginInfo>()))
            .ReturnsAsync(result ? IdentityResult.Success : IdentityResult.Failed());
    }

    private void SetupIssueUserToken(TokenDto token)
    {
        _mockUserManager.Setup(um => um.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string>());

        _mockRoleRepository.Setup(r => r.GetRoleClaimsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim>());

        _mockTokenProvider
            .Setup(tp => tp.GenerateToken(It.IsAny<User>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<Claim>>()))
            .Returns(token);
    }

    private void SetupMockAuthApiClientLogin(AuthResponseDto response)
    {
        _mockAuthApiClient
            .Setup(client => client.LoginAsync(It.IsAny<AuthRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }
}