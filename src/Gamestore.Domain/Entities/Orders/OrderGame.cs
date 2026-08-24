using Gamestore.Domain.Attributes;
using Gamestore.Domain.Serializers;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Domain.Entities.Orders;

[BsonCollection("order-details")]
public class OrderGame : BaseEntity
{
    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    [BsonElement("UnitPrice")]
    public decimal Price { get; set; }

    public int Quantity { get; set; }

    [BsonSerializer(typeof(PercentageSerializer))]
    public int? Discount { get; set; }

    [BsonElement("OrderID")]
    public int? MongoOrderId { get; set; }

    [BsonElement("ProductID")]
    public int? MongoProductId { get; set; }

    [BsonIgnore]
    public virtual Order Order { get; set; }
}