namespace Gamestore.BLL.DTOs.Games.Publishers;

public abstract record PublisherCreateUpdateDto
{
    public string CompanyName { get; init; }

    public string? HomePage { get; init; }

    public string? Description { get; init; }
}