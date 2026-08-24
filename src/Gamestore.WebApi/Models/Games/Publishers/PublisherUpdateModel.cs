namespace Gamestore.WebApi.Models.Games.Publishers;

public record PublisherUpdateModel : PublisherCreateModel
{
    public string Id { get; init; }
}