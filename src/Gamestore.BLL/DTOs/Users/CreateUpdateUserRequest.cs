namespace Gamestore.BLL.DTOs.Users;

public abstract record CreateUpdateUserRequest
{
    public IReadOnlyList<Guid> Roles { get; init; }

    public string Password { get; init; }
}