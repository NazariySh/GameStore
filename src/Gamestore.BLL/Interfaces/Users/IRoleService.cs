using Gamestore.BLL.DTOs.Users.Roles;

namespace Gamestore.BLL.Interfaces.Users;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoleDto>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<RoleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetRolePermissionsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CreateRoleResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateRoleRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}