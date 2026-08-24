namespace Gamestore.BLL.DTOs.Games;

public record CreateGameRequest : CreateUpdateGameRequest
{
    public GameCreateDto Game { get; init; }
}