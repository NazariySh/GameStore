namespace Gamestore.WebApi.Models.Auth;

public record LoginDtoModel
{
    public string Login { get; init; }

    public string Password { get; init; }

    public bool InternalAuth { get; init; }
}