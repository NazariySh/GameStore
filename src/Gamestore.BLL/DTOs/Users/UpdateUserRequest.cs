namespace Gamestore.BLL.DTOs.Users;

public record UpdateUserRequest : CreateUpdateUserRequest
{
    public UserUpdateDto User { get; init; }
}