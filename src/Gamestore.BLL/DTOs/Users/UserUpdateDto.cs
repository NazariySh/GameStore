namespace Gamestore.BLL.DTOs.Users;

public record UserUpdateDto : UserCreateUpdateDto
{
    public Guid Id { get; init; }
}