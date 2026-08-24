using System.Diagnostics.CodeAnalysis;
using Gamestore.BLL.DTOs.Logging;
using Gamestore.BLL.DTOs.Users;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Logging;
using Gamestore.BLL.Interfaces.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Users;
using Gamestore.Domain.Enums;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Shared;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services.Users;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IValidationService _validationService;
    private readonly IEntityChangeLogService _entityChangeLogService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IServiceContext context,
        UserManager<User> userManager,
        IEntityChangeLogService entityChangeLogService,
        ILogger<UserService> logger)
    {
        _unitOfWork = context.UnitOfWork;
        _userRepository = _unitOfWork.Repositories.GetGeneric<User>();
        _roleRepository = _unitOfWork.Repositories.GetGeneric<Role>();
        _mapper = context.Mapper;
        _validationService = context.Validation;
        _userManager = userManager;
        _entityChangeLogService = entityChangeLogService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve all users");

        var users = await _userRepository.GetAllAsync<UserDto>(cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} users from database", users.Count);

        return users;
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve user with Id '{Id}'", id);

        Guard.AgainstEmpty(id);

        var user = await _userRepository.GetByIdAsync<UserDto>(id, cancellationToken);

        EnsureUserExists(user, id);

        _logger.LogInformation("Retrieved user with Id '{Id}' from database", id);

        return user;
    }

    public async Task<CreateUserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = request.User;

        _logger.LogInformation("Attempting to create new user with username '{UserName}'", user.Name);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var newUser = _mapper.Map<User>(user);

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await _userManager.CreateAsync(newUser, request.Password);
            EnsureSucceeded(result, "Failed to create user");

            await AddUserToRolesAsync(newUser, request.Roles, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<User>(LogAction.Add, newVersion: newUser), cancellationToken);

            _logger.LogInformation("User with username '{UserName}' created successfully", user.Name);

            return new CreateUserResponse
            {
                Id = newUser.Id,
                UserName = newUser.UserName!,
            };
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = request.User;

        _logger.LogInformation("Attempting to update user with Id '{Id}'", user.Id);

        Guard.AgainstEmpty(user.Id);

        await EnsureUserExistsAsync(user.Id, cancellationToken);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var existingUser = await GetUserOrThrowAsync(user.Id);
        var oldVersion = _mapper.Map<User>(existingUser);

        _mapper.Map(user, existingUser);

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await _userManager.UpdateAsync(existingUser);
            EnsureSucceeded(result, "Failed to update user");

            await ResetPasswordAsync(existingUser, request.Password);
            await UpdateUserRolesAsync(existingUser, request.Roles, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<User>(LogAction.Update, oldVersion, existingUser), cancellationToken);

            _logger.LogInformation("User with Id '{Id}' updated successfully", user.Id);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete user with Id '{Id}'", id);

        Guard.AgainstEmpty(id);

        var user = await GetUserOrThrowAsync(id);

        var result = await _userManager.DeleteAsync(user);
        EnsureSucceeded(result, "Failed to delete user");

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<User>(LogAction.Delete, oldVersion: user), cancellationToken);

        _logger.LogInformation("User with Id '{Id}' deleted successfully", id);
    }

    private async Task AddUserToRolesAsync(User user, IEnumerable<Guid> roles, CancellationToken cancellationToken)
    {
        var roleNames = await GetRoleNamesAsync(roles, cancellationToken);

        await AddToRolesAsync(user, roleNames);
    }

    private async Task UpdateUserRolesAsync(User user, IEnumerable<Guid> roles, CancellationToken cancellationToken)
    {
        var roleNames = await GetRoleNamesAsync(roles, cancellationToken);
        var currentRoles = await _userManager.GetRolesAsync(user);

        var rolesToRemove = currentRoles.Except(roleNames).ToList();
        var rolesToAdd = roleNames.Except(currentRoles).ToList();

        await RemoveFromRolesAsync(user, rolesToRemove);
        await AddToRolesAsync(user, rolesToAdd);

        _logger.LogInformation("User with Id '{Id}' updated roles to: {Roles}", user.Id, string.Join(", ", roleNames));
    }

    private async Task AddToRolesAsync(User user, IReadOnlyList<string> roleNames)
    {
        var result = await _userManager.AddToRolesAsync(user, roleNames);
        EnsureSucceeded(result, "Failed to add user to roles");

        _logger.LogInformation("User with Id '{Id}' added to roles: {Roles}", user.Id, string.Join(", ", roleNames));
    }

    private async Task RemoveFromRolesAsync(User user, ICollection<string> roleNames)
    {
        var result = await _userManager.RemoveFromRolesAsync(user, roleNames);
        EnsureSucceeded(result, "Failed to remove user from roles");

        _logger.LogInformation("User with Id '{Id}' removed from roles: {Roles}", user.Id, string.Join(", ", roleNames));
    }

    private async Task ResetPasswordAsync(User user, string newPassword)
    {
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
        EnsureSucceeded(result, "Failed to reset password");

        _logger.LogInformation("Password for user with Id '{Id}' reset successfully", user.Id);
    }

    private Task<IReadOnlyList<string>> GetRoleNamesAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken)
    {
        return _roleRepository.GetAllSelectedAsync(
            r => roleIds.Contains(r.Id),
            r => r.Name!,
            cancellationToken);
    }

    private async Task<User> GetUserOrThrowAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        EnsureUserExists(user, id);

        return user;
    }

    private async Task EnsureUserExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await UserExistsAsync(id, cancellationToken))
        {
            ThrowUserNotFound(id);
        }
    }

    private void EnsureUserExists<T>([NotNull] T? user, Guid id)
    {
        if (user is null)
        {
            ThrowUserNotFound(id);
        }
    }

    private Task<bool> UserExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return _userRepository.ExistsAsync(u => u.Id == id, cancellationToken);
    }

    [DoesNotReturn]
    private void ThrowUserNotFound(Guid id)
    {
        _logger.LogError("User with Id '{Id}' not found", id);
        throw new NotFoundException(nameof(User), id);
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