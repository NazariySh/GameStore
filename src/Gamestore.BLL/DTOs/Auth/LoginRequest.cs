namespace Gamestore.BLL.DTOs.Auth;

public record LoginRequest
{
    public string Login { get; init; }

    public string Password { get; init; }
}