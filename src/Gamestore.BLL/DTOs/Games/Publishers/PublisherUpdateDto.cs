using Gamestore.Domain.Shared;

namespace Gamestore.BLL.DTOs.Games.Publishers;

public record PublisherUpdateDto : PublisherCreateUpdateDto
{
    public EntityId Id { get; init; }
}