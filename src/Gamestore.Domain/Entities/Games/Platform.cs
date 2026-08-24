using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Domain.Entities.Games;

public class Platform : BaseEntity
{
    public string Type { get; set; }

    [BsonIgnore]
    public virtual ICollection<GamePlatform> GamePlatforms { get; set; } = [];
}