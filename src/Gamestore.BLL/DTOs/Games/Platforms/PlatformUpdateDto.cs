namespace Gamestore.BLL.DTOs.Games.Platforms;

public record PlatformUpdateDto : PlatformCreateUpdateDto
{
    public Guid Id { get; init; }
}