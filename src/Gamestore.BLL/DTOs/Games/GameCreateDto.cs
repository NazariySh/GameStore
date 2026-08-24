namespace Gamestore.BLL.DTOs.Games;

public record GameCreateDto : GameCreateUpdateDto
{
    public string? Key { get; init; }
}