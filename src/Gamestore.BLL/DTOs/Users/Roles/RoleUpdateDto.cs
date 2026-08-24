namespace Gamestore.BLL.DTOs.Users.Roles;

public record RoleUpdateDto : RoleCreateUpdateDto
{
    public Guid Id { get; init; }
}