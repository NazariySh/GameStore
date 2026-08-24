namespace Gamestore.BLL.DTOs.Users.Roles;

public record RoleDto
{
    public Guid Id { get; init; }

    public string Name { get; init; }
}