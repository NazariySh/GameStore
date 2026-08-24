namespace Gamestore.BLL.DTOs.Users.Roles;

public abstract record RoleCreateUpdateDto
{
    public string Name { get; init; }
}