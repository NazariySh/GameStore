namespace Gamestore.WebApi.Models.Games;

public record CreateGameRequestModel : CreateUpdateGameRequestModel
{
    public GameCreateModel Game { get; init; }
}