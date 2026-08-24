using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Gamestore.BLL.DTOs.Logging;
using Gamestore.BLL.DTOs.Users.Roles;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Logging;
using Gamestore.BLL.Interfaces.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Repositories.Interfaces.Users;
using Gamestore.Domain.Constants;
using Gamestore.Domain.Entities.Users;
using Gamestore.Domain.Enums;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Shared;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services.Users;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRoleRepository _roleRepository;
    private readonly RoleManager<Role> _roleManager;
    private readonly IMapper _mapper;
    private readonly IValidationService _validationService;
    private readonly IEntityChangeLogService _entityChangeLogService;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        IServiceContext context,
        RoleManager<Role> roleManager,
        IEntityChangeLogService entityChangeLogService,
        ILogger<RoleService> logger)
    {
        _unitOfWork = context.UnitOfWork;
        _roleRepository = _unitOfWork.Repositories.Get<IRoleRepository>();
        _mapper = context.Mapper;
        _validationService = context.Validation;
        _roleManager = roleManager;
        _entityChangeLogService = entityChangeLogService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve all roles");

        var roles = await _roleRepository.GetAllAsync<RoleDto>(cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} roles from database", roles.Count);

        return roles;
    }

    public async Task<IReadOnlyList<RoleDto>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve roles for user with Id '{UserId}'", userId);

        Guard.AgainstEmpty(userId);

        var userRoles = await _roleRepository.GetAllAsync<RoleDto>(
            r => r.UserRoles.Any(ur => ur.UserId == userId),
            cancellationToken);

        _logger.LogInformation("Retrieved {Count} roles for user with Id '{UserId}'", userRoles.Count, userId);

        return userRoles;
    }

    public async Task<RoleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve role with Id '{Id}'", id);

        Guard.AgainstEmpty(id);

        var role = await _roleRepository.GetByIdAsync<RoleDto>(id, cancellationToken);

        EnsureRoleExists(role, id);

        _logger.LogInformation("Retrieved role with Id '{Id}' from database", id);

        return role;
    }

    public async Task<IReadOnlyList<string>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve all permissions");

        var permissions = await _roleRepository.GetAllPermissionsAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} permissions from database", permissions.Count);

        return permissions;
    }

    public async Task<IReadOnlyList<string>> GetRolePermissionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve permissions for role with Id '{Id}'", id);

        Guard.AgainstEmpty(id);

        var permissions = await _roleRepository.GetRolePermissionsAsync(id, cancellationToken);

        _logger.LogInformation("Retrieved {Count} permissions for role with Id '{Id}'", permissions.Count, id);

        return permissions;
    }

    public async Task<CreateRoleResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = request.Role;

        _logger.LogInformation("Attempting to create a new role with name '{RoleName}'", role.Name);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var newRole = _mapper.Map<Role>(role);

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await _roleManager.CreateAsync(newRole);
            EnsureSucceeded(result, "Failed to create role");

            await AddPermissionsAsync(newRole, request.Permissions);

            await transaction.CommitAsync(cancellationToken);

            await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Role>(LogAction.Add, newVersion: newRole), cancellationToken);

            _logger.LogInformation("Role '{RoleName}' created successfully", role.Name);

            return new CreateRoleResponse
            {
                Id = newRole.Id,
                Name = newRole.Name!,
            };
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateAsync(UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = request.Role;

        _logger.LogInformation("Attempting to update role with Id '{RoleId}'", role.Id);

        Guard.AgainstEmpty(role.Id);

        await EnsureRoleExistsAsync(role.Id, cancellationToken);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var existingRole = await GetRoleOrThrowAsync(role.Id);
        var oldVersion = _mapper.Map<Role>(existingRole);

        _mapper.Map(role, existingRole);

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await _roleManager.UpdateAsync(existingRole);
            EnsureSucceeded(result, "Failed to update role");

            await UpdatePermissionsAsync(existingRole, request.Permissions);

            await transaction.CommitAsync(cancellationToken);

            await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Role>(LogAction.Update, oldVersion, existingRole), cancellationToken);

            _logger.LogInformation("Role with Id '{RoleId}' updated successfully", role.Id);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete role with Id '{Id}'", id);

        Guard.AgainstEmpty(id);

        var role = await GetRoleOrThrowAsync(id);

        var result = await _roleManager.DeleteAsync(role);
        EnsureSucceeded(result, "Failed to delete role");

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Role>(LogAction.Delete, oldVersion: role), cancellationToken);

        _logger.LogInformation("Role with Id '{Id}' deleted successfully", id);
    }

    private async Task AddPermissionsAsync(Role role, IReadOnlyList<string> permissions)
    {
        foreach (var permission in permissions)
        {
            var result = await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, permission));
            EnsureSucceeded(result, $"Failed to add permission '{permission}' to role '{role.Name}'");
        }

        _logger.LogInformation("Added {Count} permissions to role '{RoleName}'", permissions.Count, role.Name);
    }

    private async Task RemovePermissionsAsync(Role role, List<Claim> permissions)
    {
        foreach (var permission in permissions)
        {
            var result = await _roleManager.RemoveClaimAsync(role, permission);
            EnsureSucceeded(result, $"Failed to remove permission '{permission.Value}' from role '{role.Name}'");
        }

        _logger.LogInformation("Removed {Count} permissions from role '{RoleName}'", permissions.Count, role.Name);
    }

    private async Task UpdatePermissionsAsync(Role role, IReadOnlyList<string> permissions)
    {
        var currentClaims = await _roleManager.GetClaimsAsync(role);
        var currentPermissions = currentClaims
            .Where(c => c.Type == CustomClaimTypes.Permission)
            .ToList();

        var permissionsToRemove = currentPermissions
            .Where(c => !permissions.Contains(c.Value))
            .ToList();

        var permissionsToAdd = permissions
            .Where(p => currentPermissions.All(c => c.Value != p))
            .ToList();

        await RemovePermissionsAsync(role, permissionsToRemove);
        await AddPermissionsAsync(role, permissionsToAdd);

        _logger.LogInformation("Updated permissions for role '{RoleName}'", role.Name);
    }

    private async Task<Role> GetRoleOrThrowAsync(Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());

        EnsureRoleExists(role, id);

        return role;
    }

    private async Task EnsureRoleExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await RoleExistsAsync(id, cancellationToken))
        {
            ThrowRoleNotFound(id);
        }
    }

    private void EnsureRoleExists<T>([NotNull] T? role, Guid id)
    {
        if (role is null)
        {
            ThrowRoleNotFound(id);
        }
    }

    private Task<bool> RoleExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return _roleRepository.ExistsAsync(r => r.Id == id, cancellationToken);
    }

    [DoesNotReturn]
    private void ThrowRoleNotFound(Guid id)
    {
        _logger.LogWarning("Role with Id '{Id}' not found", id);
        throw new NotFoundException(nameof(Role), id);
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