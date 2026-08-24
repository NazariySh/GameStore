namespace Gamestore.BLL.DTOs.Games.Platforms;

public abstract record PlatformCreateUpdateDto
{
    public string Type { get; init; }
}