namespace Gamestore.WebApi.Models.Auth;

public record LoginRequestModel
{
    public LoginDtoModel Model { get; init; }
}