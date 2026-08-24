using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Domain.Entities;

[BsonIgnoreExtraElements(Inherited = true)]
public abstract class BaseEntity : IBaseEntity
{
    public Guid Id { get; set; }

    [NotMapped]
    [BsonId]
    public ObjectId ObjectId { get; set; }
}