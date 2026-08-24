using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Domain.Entities.Games;

public class GameGenre
{
    public Guid GameId { get; set; }

    public Guid GenreId { get; set; }

    [NotMapped]
    [BsonElement("CategoryID")]
    public int? CategoryId { get; set; }

    [BsonIgnore]
    public virtual Game Game { get; set; }

    [BsonIgnore]
    public virtual Genre Genre { get; set; }
}