using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Domain.Entities.Games;

public class GamePlatform
{
    public Guid GameId { get; set; }

    public Guid PlatformId { get; set; }

    [BsonIgnore]
    public virtual Game Game { get; set; }

    [BsonIgnore]
    public virtual Platform Platform { get; set; }
}