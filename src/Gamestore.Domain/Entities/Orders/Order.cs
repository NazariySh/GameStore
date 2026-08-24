using Gamestore.Domain.Attributes;
using Gamestore.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Domain.Entities.Orders;

[BsonCollection("orders")]
public class Order : BaseEntity
{
    [BsonElement("OrderDate")]
    public DateTime? Date { get; set; }

    public Guid CustomerId { get; set; }

    public OrderStatus Status { get; set; }

    [BsonElement("OrderID")]
    public int? MongoOrderId { get; set; }

    [BsonElement("CustomerID")]
    public string? MongoCustomerId { get; set; }

    [BsonElement("EmployeeID")]
    public int EmployeeId { get; set; }

    public DateTime? RequiredDate { get; set; }

    public DateTime? ShippedDate { get; set; }

    public int ShipVia { get; set; }

    public double Freight { get; set; }

    public string? ShipName { get; set; }

    public string? ShipAddress { get; set; }

    public string? ShipCity { get; set; }

    public string? ShipRegion { get; set; }

    public string? ShipPostalCode { get; set; }

    public string? ShipCountry { get; set; }

    [BsonIgnore]
    public virtual ICollection<OrderGame> OrderGames { get; set; } = [];
}