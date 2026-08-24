namespace Gamestore.BLL.DTOs.Users.Roles;

public abstract record CreateUpdateRoleRequest
{
    public IReadOnlyList<string> Permissions { get; init; }
}