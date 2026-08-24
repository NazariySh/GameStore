using System.Security.Claims;
using Gamestore.BLL.DTOs.Auth;
using Gamestore.BLL.Integrations.Interfaces;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Auth;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Repositories.Interfaces.Users;
using Gamestore.Domain.Entities.Users;
using Gamestore.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services.Auth;

public class AuthService : IAuthService
{
    private const string ExternalAuthLoginProvider = "AuthService";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRoleRepository _roleRepository;
    private readonly UserManager<User> _userManager;
    private readonly IValidationService _validationService;
    private readonly ITokenProvider _tokenProvider;
    private readonly IAuthApiClient _authApiClient;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IServiceContext context,
        UserManager<User> userManager,
        ITokenProvider tokenProvider,
        IAuthApiClient authApiClient,
        ILogger<AuthService> logger)
    {
        _unitOfWork = context.UnitOfWork;
        _roleRepository = _unitOfWork.Repositories.Get<IRoleRepository>();
        _validationService = context.Validation;
        _userManager = userManager;
        _tokenProvider = tokenProvider;
        _authApiClient = authApiClient;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to log in user with login '{Login}'", request.Login);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var user = await _userManager.FindByNameAsync(request.Login);

        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogError("Failed login attempt for user with login '{Login}'", request.Login);
            throw new ArgumentException("Invalid login or password");
        }

        var token = await IssueUserTokenAsync(user, cancellationToken);

        _logger.LogInformation("User with login '{Login}' logged in successfully", request.Login);

        return new LoginResponse
        {
            Token = token.AccessToken,
        };
    }

    public async Task<LoginResponse> LoginWithExternalAuthAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to log in user with external auth for login '{Login}'", request.Login);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var authRequest = new AuthRequestDto
        {
            Email = request.Login,
            Password = request.Password,
        };

        var authResponse = await _authApiClient.LoginAsync(authRequest, cancellationToken);

        var user = await _userManager.FindByEmailAsync(authResponse.Email);

        if (user is null)
        {
            user = new User
            {
                UserName = authResponse.Email,
                Email = authResponse.Email,
                FirstName = authResponse.FirstName,
                LastName = authResponse.LastName,
            };

            var loginInfo = new UserLoginInfo(
                ExternalAuthLoginProvider,
                authResponse.Email,
                ExternalAuthLoginProvider);

            await RegisterUserWithExternalLoginAsync(user, loginInfo, cancellationToken);
        }

        var token = await IssueUserTokenAsync(user, cancellationToken);

        _logger.LogInformation("User with email '{Email}' logged in successfully via external auth", authResponse.Email);

        return new LoginResponse
        {
            Token = token.AccessToken,
        };
    }

    private async Task RegisterUserWithExternalLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await _userManager.CreateAsync(user);
            EnsureSucceeded(result, "Failed to create user");

            await AddUserToRoleAsync(user, RoleType.User);
            await AddUserLoginAsync(user, login);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("New user with Id '{Id}' registered successfully via external auth", user.Id);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task AddUserToRoleAsync(User user, RoleType roleType)
    {
        var result = await _userManager.AddToRoleAsync(user, roleType.ToString());
        EnsureSucceeded(result, $"Failed to add role '{roleType}' to user");

        _logger.LogInformation("User with Id '{Id}' added to role '{Role}'", user.Id, roleType);
    }

    private async Task AddUserLoginAsync(User user, UserLoginInfo login)
    {
        var result = await _userManager.AddLoginAsync(user, login);
        EnsureSucceeded(result, "Failed to add external login to user");

        _logger.LogInformation("External login added for user with Id '{Id}'", user.Id);
    }

    private async Task<TokenDto> IssueUserTokenAsync(User user, CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var rolesClaims = await GetRolesClaimsAsync(roles, cancellationToken);
        return _tokenProvider.GenerateToken(user, roles, rolesClaims);
    }

    private async Task<List<Claim>> GetRolesClaimsAsync(ICollection<string> roles, CancellationToken cancellationToken)
    {
        var claims = new List<Claim>();

        foreach (var role in roles)
        {
            var roleClaims = await _roleRepository.GetRoleClaimsAsync(role, cancellationToken);
            claims.AddRange(roleClaims);
        }

        return claims.DistinctBy(c => new { c.Type, c.Value }).ToList();
    }

    private void EnsureSucceeded(IdentityResult result, string errorMessage)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Operation failed: {ErrorMessage}. Errors: {Errors}", errorMessage, errors);
            throw new InvalidOperationException($"{errorMessage}: {errors}");
        }
    }
}