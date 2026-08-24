using System.ComponentModel.DataAnnotations.Schema;
using Gamestore.Domain.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Domain.Entities.Games;

[BsonCollection("categories")]
public class Genre : BaseEntity
{
    [BsonElement("CategoryName")]
    public string Name { get; set; }

    public Guid? ParentGenreId { get; set; }

    [BsonElement("CategoryID")]
    public int? CategoryId { get; set; }

    public string? Description { get; set; }

    public string? Picture { get; set; }

    [NotMapped]
    [BsonElement("ParentCategoryID")]
    public int? ParentCategoryId { get; set; }

    [BsonIgnore]
    public virtual Genre? ParentGenre { get; set; }

    [BsonIgnore]
    public virtual ICollection<Genre> SubGenres { get; set; } = [];

    [BsonIgnore]
    public virtual ICollection<GameGenre> GameGenres { get; set; } = [];
}