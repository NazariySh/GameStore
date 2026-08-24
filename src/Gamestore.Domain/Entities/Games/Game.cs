using Gamestore.Domain.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Domain.Entities.Games;

[BsonCollection("products")]
public class Game : BaseEntity, ISoftDeletable
{
    [BsonElement("ProductName")]
    public string Name { get; set; }

    public string Key { get; set; }

    public string? Description { get; set; }

    [BsonElement("UnitPrice")]
    public decimal Price { get; set; }

    [BsonElement("UnitsInStock")]
    public int UnitInStock { get; set; }

    [BsonElement("Discontinued")]
    public int Discount { get; set; }

    public int ViewCount { get; set; }

    public int CommentCount { get; set; }

    public Guid PublisherId { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    [BsonElement("ProductID")]
    public int? ProductId { get; set; }

    [BsonElement("SupplierID")]
    public int? SupplierId { get; set; }

    [BsonElement("CategoryID")]
    public int? CategoryId { get; set; }

    public string? QuantityPerUnit { get; set; }

    public int UnitsOnOrder { get; set; }

    public int ReorderLevel { get; set; }

    [BsonIgnore]
    public virtual Publisher Publisher { get; set; }

    public virtual ICollection<GameGenre> GameGenres { get; set; } = [];

    public virtual ICollection<GamePlatform> GamePlatforms { get; set; } = [];
}