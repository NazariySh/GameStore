namespace Gamestore.BLL.DTOs.Games.Platforms;

public record UpdatePlatformRequest
{
    public PlatformUpdateDto Platform { get; init; }
}