namespace Gamestore.BLL.DTOs.Users.Roles;

public record CreateRoleRequest : CreateUpdateRoleRequest
{
    public RoleCreateDto Role { get; init; }
}