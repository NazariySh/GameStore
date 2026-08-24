namespace Gamestore.BLL.DTOs.Users.Roles;

public record UpdateRoleRequest : CreateUpdateRoleRequest
{
    public RoleUpdateDto Role { get; init; }
}