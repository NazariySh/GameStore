namespace Gamestore.WebApi.Models.Games.Publishers;

public record UpdatePublisherRequestModel
{
    public PublisherUpdateModel Publisher { get; init; }
}