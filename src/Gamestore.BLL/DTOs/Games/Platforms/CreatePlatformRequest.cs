namespace Gamestore.BLL.DTOs.Games.Platforms;

public record CreatePlatformRequest
{
    public PlatformCreateDto Platform { get; init; }
}