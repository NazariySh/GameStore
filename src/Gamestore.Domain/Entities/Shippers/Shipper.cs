using Gamestore.Domain.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Domain.Entities.Shippers;

[BsonCollection("shippers")]
public class Shipper : BaseEntity
{
    [BsonElement("ShipperID")]
    public int ShipperId { get; set; }

    public string CompanyName { get; set; }

    public string Phone { get; set; }
}