namespace Gamestore.BLL.DTOs.Games.Publishers;

public record CreatePublisherRequest
{
    public PublisherCreateDto Publisher { get; init; }
}