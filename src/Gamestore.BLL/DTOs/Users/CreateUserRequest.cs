namespace Gamestore.BLL.DTOs.Users;

public record CreateUserRequest : CreateUpdateUserRequest
{
    public UserCreateDto User { get; init; }
}