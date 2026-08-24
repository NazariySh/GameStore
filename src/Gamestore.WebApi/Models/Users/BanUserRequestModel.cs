namespace Gamestore.WebApi.Models.Users;

public record BanUserRequestModel
{
    public string User { get; init; }

    public string Duration { get; init; }
}