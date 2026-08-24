namespace Gamestore.BLL.DTOs.Auth;

public record AuthRequestDto
{
    public string Email { get; init; }

    public string Password { get; init; }
}