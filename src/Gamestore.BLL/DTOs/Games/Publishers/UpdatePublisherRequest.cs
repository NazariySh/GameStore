namespace Gamestore.BLL.DTOs.Games.Publishers;

public record UpdatePublisherRequest
{
    public PublisherUpdateDto Publisher { get; init; }
}