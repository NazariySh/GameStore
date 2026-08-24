namespace Gamestore.BLL.DTOs.Games.Platforms;

public record PlatformDto
{
    public Guid Id { get; init; }

    public string Type { get; init; }
}