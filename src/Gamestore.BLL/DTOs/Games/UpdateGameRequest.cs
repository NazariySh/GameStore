namespace Gamestore.BLL.DTOs.Games;

public record UpdateGameRequest : CreateUpdateGameRequest
{
    public GameUpdateDto Game { get; init; }
}