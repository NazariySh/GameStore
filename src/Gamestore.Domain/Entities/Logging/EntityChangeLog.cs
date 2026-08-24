using Gamestore.Domain.Attributes;
using MongoDB.Bson;

namespace Gamestore.Domain.Entities.Logging;

[BsonCollection("entity-change-logs")]
public class EntityChangeLog : BaseEntity
{
    public DateTime Date { get; set; }

    public string Action { get; set; }

    public string EntityType { get; set; }

    public BsonDocument? OldVersion { get; set; }

    public BsonDocument? NewVersion { get; set; }
}