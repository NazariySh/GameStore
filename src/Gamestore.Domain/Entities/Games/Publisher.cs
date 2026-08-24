using Gamestore.Domain.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Domain.Entities.Games;

[BsonCollection("suppliers")]
public class Publisher : BaseEntity
{
    public string CompanyName { get; set; }

    public string? HomePage { get; set; }

    public string? Description { get; set; }

    [BsonElement("SupplierID")]
    public int? SupplierId { get; set; }

    public string? ContactName { get; set; }

    public string? ContactTitle { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Region { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public string? Phone { get; set; }

    public string? Fax { get; set; }

    [BsonIgnore]
    public virtual ICollection<Game> Games { get; set; } = [];
}