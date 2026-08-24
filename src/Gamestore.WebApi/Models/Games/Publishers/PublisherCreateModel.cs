namespace Gamestore.WebApi.Models.Games.Publishers;

public record PublisherCreateModel
{
    public string CompanyName { get; init; }

    public string? HomePage { get; init; }

    public string? Description { get; init; }
}