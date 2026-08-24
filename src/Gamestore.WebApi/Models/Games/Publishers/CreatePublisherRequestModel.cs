namespace Gamestore.WebApi.Models.Games.Publishers;

public record CreatePublisherRequestModel
{
    public PublisherCreateModel Publisher { get; init; }
}