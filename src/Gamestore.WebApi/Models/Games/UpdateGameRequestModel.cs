namespace Gamestore.WebApi.Models.Games;

public record UpdateGameRequestModel : CreateUpdateGameRequestModel
{
    public GameUpdateModel Game { get; init; }
}