namespace Gamestore.BLL.DTOs.Users;

public abstract record UserCreateUpdateDto
{
    public string Name { get; init; }
}