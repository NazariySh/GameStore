namespace Gamestore.BLL.DTOs.Users;

public record UserDto
{
    public Guid Id { get; init; }

    public string Name { get; init; }
}